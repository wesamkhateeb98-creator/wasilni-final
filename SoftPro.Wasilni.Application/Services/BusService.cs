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
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Services;

public class BusService(IUnitOfWork unitOfWork, IMemoryCache cache) : IBusService
{
    // ─── Admin CRUD ───────────────────────────────────────────────────────────

    public Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken)
        => unitOfWork.BusRepository.GetAllBusesForAdminAsync(inputModel, cancellationToken);

    public async Task<int> AddAsync(AddBusModel model, CancellationToken cancellationToken)
    {
        if (await unitOfWork.BusRepository.ExistsPlateAsync(model.Plate, cancellationToken))
            throw new AlreadyExistsException(Phrases.PlateAlreadyExists);

        if (model.LineId.HasValue && !await unitOfWork.LineRepository.AnyAsync(model.LineId.Value, cancellationToken))
            throw new FailedPreconditionException(Phrases.LineNotFound);

        BusEntity bus = BusEntity.Create(model);
        await unitOfWork.BusRepository.AddAsync(bus, cancellationToken);
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

        if (model.LineId.HasValue && bus.LineId != model.LineId)
            if (!await unitOfWork.LineRepository.AnyAsync(model.LineId.Value, cancellationToken))
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

    public async Task<int> AddDriverAsync(int busId, int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(busId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is not null)
            throw new FailedPreconditionException(Phrases.AssignedOtherDriver);

        if (await unitOfWork.AccountRepository.GetByIdAsync(driverId, cancellationToken) is null)
            throw new NotFoundException(Phrases.DriverNotFound);

        bus.AssignDriverId(driverId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    public async Task<int> DeleteDriverAsync(int busId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(busId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is null)
            throw new FailedPreconditionException(Phrases.DriverNotFound);

        int driverId = bus.DriverId.Value;
        bus.UnassignDriver();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Remove(BusCacheKeys.DriverInfo(driverId));

        return bus.Id;
    }

    // ─── Driver: Bus state ────────────────────────────────────────────────────

    public async Task<DriverBusInfoModel> GetBusInfoAsync(int driverId, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(BusCacheKeys.DriverInfo(driverId), async _ =>
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            return new DriverBusInfoModel(
                bus.Id,
                bus.Plate,
                bus.Color,
                bus.Type,
                bus.Status,
                bus.LineId,
                bus.LineEntity?.Name);
        }) ?? throw new NotFoundException(Phrases.BusNotFound);
    }

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
            if (bus.LineId is null)
                throw new FailedPreconditionException(Phrases.LineNotFound);

            bus.Activate();
            await unitOfWork.CompleteAsync(cancellationToken);

            cache.Set(BusCacheKeys.DriverBus(driverId), bus.Id);
            cache.Set(BusCacheKeys.DriverLine(driverId), bus.LineId);

            return bus.ToModel(null);
        }
    }

    public async Task<UpdateLocationResult> UpdateLocationAsync(UpdateBusLocationModel model, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(model.DriverId), out int busId) ||
            !cache.TryGetValue(BusCacheKeys.DriverLine(model.DriverId), out int lineId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(model.DriverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            busId = bus.Id;
            lineId = bus.LineId!.Value;
            cache.Set(BusCacheKeys.DriverBus(model.DriverId), busId);
            cache.Set(BusCacheKeys.DriverLine(model.DriverId), lineId);
        }

        cache.Set(BusCacheKeys.Location(busId), new BusLocationModel(model.Latitude, model.Longitude, DateTime.UtcNow));
        return new UpdateLocationResult(busId, lineId);
    }

    public async Task<AdjustAnonymousResult> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId) ||
            !cache.TryGetValue(BusCacheKeys.DriverLine(driverId), out int lineId))
        {
            BusEntity cached = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (cached.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            busId = cached.Id;
            lineId = cached.LineId!.Value;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
            cache.Set(BusCacheKeys.DriverLine(driverId), lineId);
        }

        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(busId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        bus.AdjustAnonymous(delta);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new AdjustAnonymousResult(bus.Id, lineId, bus.AnonymousCount);
    }

    public async Task<int> ConfirmRiderAsync(int driverId, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId) ||
            !cache.TryGetValue(BusCacheKeys.DriverLine(driverId), out int lineId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);
            busId = bus.Id;
            lineId = bus.LineId!.Value;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
            cache.Set(BusCacheKeys.DriverLine(driverId), lineId);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await unitOfWork.DailyRidershipRepository.IncrementAsync(new IncrementRidershipModel(lineId, busId, today), cancellationToken);
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

            busId = bus.Id;
            lineId = bus.LineId!.Value;
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

    public async Task<BookingActionResult> ConfirmBookingAsync(
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
        await unitOfWork.DailyRidershipRepository.IncrementAsync(new IncrementRidershipModel(booking.LineId, busId, today), cancellationToken);

        return new BookingActionResult(booking.Id, booking.LineId);
    }

    public async Task<BookingActionResult> MarkNoShowAsync(
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

            driverLineId = bus.LineId!.Value;
            cache.Set(BusCacheKeys.DriverBus(driverId), bus.Id);
            cache.Set(BusCacheKeys.DriverLine(driverId), driverLineId);
        }

        if (booking.LineId != driverLineId)
            throw new ForbiddenException(Phrases.Forbidden);

        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);

        return new BookingActionResult(booking.Id, booking.LineId);
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

    public async Task<MyBookingResult?> GetMyBookingAsync(int passengerId, CancellationToken cancellationToken)
    {
        BookingEntity? booking = await unitOfWork.BookingRepository
            .GetActiveByPassengerWithLineAsync(passengerId, cancellationToken);

        if (booking is null) return null;
        return new MyBookingResult(booking.Id, booking.LineId, booking.Line.Name);
    }

    public async Task<int> AddBookingAsync(CreateBookingModel model, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.LineRepository.AnyAsync(model.LineId, cancellationToken))
            throw new NotFoundException(Phrases.LineNotFound);

        if (await unitOfWork.BookingRepository.HasActiveBookingAsync(model.PassengerId, cancellationToken))
            throw new AlreadyExistsException(Phrases.AlreadyBooked);

        BookingEntity booking = BookingEntity.Create(model.LineId, model.PassengerId, model.Latitude, model.Longitude);
        await unitOfWork.BookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return booking.Id;
    }

    public async Task<BookingActionResult> CancelBookingAsync(int passengerId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository
            .GetActiveByPassengerAsync(passengerId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        int lineId = booking.LineId;
        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);
        return new BookingActionResult(booking.Id, lineId);
    }
}
