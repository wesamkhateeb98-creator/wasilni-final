using Domain.Resources;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
using SoftPro.Wasilni.Application.Extensions;
using SoftPro.Wasilni.Application.Helpers;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Services;

public class BusService(IUnitOfWork unitOfWork, IMemoryCache cache) : IBusService
{
    // ─── Admin CRUD ───────────────────────────────────────────────────────────

    public Task<Page<GetBusesModel>> GetBusesAsync(GetBusModel inputModel, CancellationToken cancellationToken)
        => unitOfWork.BusRepository.GetAllBusesForPassengerIdAsync(inputModel, cancellationToken);

    public Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken)
        => unitOfWork.BusRepository.GetAllBusesForAdminAsync(inputModel, cancellationToken);

    public async Task<int> RegisterAsync(RegisterBusModel registerModel, CancellationToken cancellationToken)
    {
        AccountEntity? account = await unitOfWork.AccountRepository.GetByIdAsync(registerModel.accountId, cancellationToken)
            ?? throw new NotFoundException(Phrases.AccountNotRegistered);

        if (await unitOfWork.BusRepository.ExistsPlateAsync(registerModel.Plate, cancellationToken))
            throw new AlreadyExistsException(Phrases.PlateAlreadyExists);

        if (!await unitOfWork.LineRepository.AnyAsync(registerModel.lineId, cancellationToken))
            throw new FailedPreconditionException(Phrases.LineNotFound);

        BusEntity bus = BusEntity.Create(registerModel);
        await unitOfWork.BusRepository.AddAsync(bus, cancellationToken);
        account.ChangePermission(Permission.ControlBus);
        await unitOfWork.CompleteAsync(cancellationToken);
        return bus.Id;
    }

    public async Task<int> UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdWithDriverAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (!string.Equals(bus.Plate, model.Plate, StringComparison.OrdinalIgnoreCase))
            if (await unitOfWork.BusRepository.ExistsPlateExceptAsync(model.Plate, id, cancellationToken))
                throw new AlreadyExistsException(Phrases.PlateAlreadyExists);

        if (bus.LineId != model.LineId)
            if (!await unitOfWork.LineRepository.AnyAsync(model.LineId, cancellationToken))
                throw new FailedPreconditionException(Phrases.LineNotFound);

        bus.Update(model);
        await unitOfWork.CompleteAsync(cancellationToken);
        return bus.Id;
    }

    public async Task<int> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetWithRequestByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        await unitOfWork.Transaction(async () =>
        {
            unitOfWork.BusRepository.Delete(bus, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }, cancellationToken);

        return bus.Id;
    }

    public async Task<int> AddDriver(AddDriverOnBusModel model, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(model.BusId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is not null)
            throw new FailedPreconditionException(Phrases.AssignedOtherDriver);

        AccountEntity account = await unitOfWork.AccountRepository.GetByIdAsync(model.DriverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.DriverNotFound);

        if (account.Permission == Permission.BusDriving)
            throw new FailedPreconditionException(Phrases.DriverAlreadyExists);

        account.ChangePermission(Permission.BusDriving);
        bus.AssignDriverId(model.DriverId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    public async Task<int> DeleteDriver(DeleteDriverFromBusModel model, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(model.BusId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is null)
            throw new FailedPreconditionException(Phrases.DriverNotFound);

        AccountEntity account = await unitOfWork.AccountRepository.GetByIdAsync(model.DriverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.DriverNotFound);

        if (account.Permission != Permission.BusDriving)
            throw new FailedPreconditionException(Phrases.DriverNotFound);

        account.ChangePermission(Permission.None);
        bus.UnassignDriver();
        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    // ─── Driver: Bus state ────────────────────────────────────────────────────

    public async Task<GetActiveBusModel> ToggleStatusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.Status == BusStatus.Active)
        {
            bus.Deactivate();
            await unitOfWork.CompleteAsync(cancellationToken);

            cache.Remove(BusCacheKeys.DriverBus(driverId));
            cache.Remove(BusCacheKeys.DriverLine(driverId));
            cache.Remove(BusCacheKeys.Location(bus.Id));

            return bus.ToModel(null);
        }
        else
        {
            bus.Activate();
            await unitOfWork.CompleteAsync(cancellationToken);

            cache.Set(BusCacheKeys.DriverBus(driverId), bus.Id);
            cache.Set(BusCacheKeys.DriverLine(driverId), bus.LineId);

            return bus.ToModel(null);
        }
    }

    public async Task<(int BusId, int LineId)> UpdateLocationAsync(int driverId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId) ||
            !cache.TryGetValue(BusCacheKeys.DriverLine(driverId), out int lineId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            busId  = bus.Id;
            lineId = bus.LineId;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
            cache.Set(BusCacheKeys.DriverLine(driverId), lineId);
        }

        cache.Set(BusCacheKeys.Location(busId), new BusLocationModel(latitude, longitude, DateTime.UtcNow));
        return (busId, lineId);
    }

    public async Task<(int BusId, int Count)> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.Status != BusStatus.Active)
            throw new FailedPreconditionException(Phrases.BusNotOnRoad);

        bus.AdjustAnonymous(delta);
        await unitOfWork.CompleteAsync(cancellationToken);
        return (bus.Id, bus.AnonymousCount);
    }

    public async Task<int> ConfirmRiderAsync(int driverId, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);
            busId = bus.Id;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
            cache.Set(BusCacheKeys.DriverLine(driverId), bus.LineId);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await unitOfWork.DailyRidershipRepository.IncrementAsync(busId, today, cancellationToken);
    }

    public async Task<GetActiveBusModel?> GetMyActiveBusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity? bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken);

        if (bus is null || bus.Status != BusStatus.Active)
            return null;

        cache.Set(BusCacheKeys.DriverBus(driverId), bus.Id);
        cache.Set(BusCacheKeys.DriverLine(driverId), bus.LineId);

        var location = cache.Get<BusLocationModel>(BusCacheKeys.Location(bus.Id));
        return bus.ToModel(location);
    }

    // ─── Driver: Bookings ─────────────────────────────────────────────────────

    public async Task<List<GetBookingModel>> GetNearbyBookingsAsync(int driverId, CancellationToken cancellationToken)
    {
        // Resolve busId + lineId (prefer cache, fall back to DB once)
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId) ||
            !cache.TryGetValue(BusCacheKeys.DriverLine(driverId), out int lineId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            busId  = bus.Id;
            lineId = bus.LineId;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
            cache.Set(BusCacheKeys.DriverLine(driverId), lineId);
        }

        // Bus must have a known location (driver sends GPS via hub)
        var location = cache.Get<BusLocationModel>(BusCacheKeys.Location(busId));
        if (location is null)
            return [];

        // All waiting bookings on this line, filtered by 40 m radius
        List<BookingEntity> bookings =
            await unitOfWork.BookingRepository.GetWaitingByLineAsync(lineId, cancellationToken);

        return bookings
            .Where(b => GeoHelper.Distance(b.Latitude, b.Longitude,
                                           location.Latitude, location.Longitude) <= 40)
            .Select(b => b.ToModel())
            .ToList();
    }

    public async Task<(int BookingId, int LineId)> ConfirmBookingAsync(
        int bookingId, int driverId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        if (booking.Status != BookingStatus.Waiting)
            throw new FailedPreconditionException(Phrases.AlreadyBooked);

        // Resolve busId for daily ridership record
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);
            busId = bus.Id;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
            cache.Set(BusCacheKeys.DriverLine(driverId), bus.LineId);
        }

        booking.MarkPickedUp();
        await unitOfWork.CompleteAsync(cancellationToken);

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        await unitOfWork.DailyRidershipRepository.IncrementAsync(busId, today, cancellationToken);

        return (booking.Id, booking.LineId);
    }

    public async Task<(int BookingId, int LineId)> MarkNoShowAsync(
        int bookingId, int driverId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        if (booking.Status != BookingStatus.Waiting)
            throw new FailedPreconditionException(Phrases.AlreadyBooked);

        // Verify the driver's bus is on the same line as the booking
        if (!cache.TryGetValue(BusCacheKeys.DriverLine(driverId), out int driverLineId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            driverLineId = bus.LineId;
            cache.Set(BusCacheKeys.DriverBus(driverId), bus.Id);
            cache.Set(BusCacheKeys.DriverLine(driverId), driverLineId);
        }

        if (booking.LineId != driverLineId)
            throw new ForbiddenException(Phrases.Forbidden);

        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);

        return (booking.Id, booking.LineId);
    }

    // ─── Passenger ────────────────────────────────────────────────────────────

    public async Task<List<GetActiveBusModel>> GetActiveBusesAsync(int? lineId, CancellationToken cancellationToken)
    {
        List<BusEntity> buses = await unitOfWork.BusRepository.GetActiveBusesAsync(lineId, cancellationToken);

        return buses.Select(b =>
        {
            var location = cache.Get<BusLocationModel>(BusCacheKeys.Location(b.Id));
            return b.ToModel(location);
        }).ToList();
    }

    public async Task<int> AddBookingAsync(
        int lineId, int passengerId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.LineRepository.AnyAsync(lineId, cancellationToken))
            throw new NotFoundException(Phrases.LineNotFound);

        if (await unitOfWork.BookingRepository.HasActiveBookingOnLineAsync(passengerId, lineId, cancellationToken))
            throw new AlreadyExistsException(Phrases.AlreadyBooked);

        BookingEntity booking = BookingEntity.Create(lineId, passengerId, latitude, longitude);
        await unitOfWork.BookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return booking.Id;
    }

    public async Task<int> CancelBookingAsync(int lineId, int passengerId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository
            .GetActiveByPassengerAndLineAsync(passengerId, lineId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);
        return booking.Id;
    }
}
