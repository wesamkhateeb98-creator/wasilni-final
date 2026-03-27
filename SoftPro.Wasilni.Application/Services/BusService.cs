using Domain.Resources;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
using SoftPro.Wasilni.Application.Extensions;
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

    // ─── Driver Tracking ──────────────────────────────────────────────────────

    public async Task<GetActiveBusModel> ToggleStatusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.Status == BusStatus.Active)
        {
            bus.Deactivate();
            await unitOfWork.CompleteAsync(cancellationToken);

            cache.Remove(BusCacheKeys.DriverBus(driverId));
            cache.Remove(BusCacheKeys.Location(bus.Id));

            return bus.ToModel(null);
        }
        else
        {
            bus.Activate();
            await unitOfWork.CompleteAsync(cancellationToken);

            cache.Set(BusCacheKeys.DriverBus(driverId), bus.Id);

            return bus.ToModel(null);
        }
    }

    public async Task<int> UpdateLocationAsync(int driverId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverBus(driverId), out int busId))
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            busId = bus.Id;
            cache.Set(BusCacheKeys.DriverBus(driverId), busId);
        }

        cache.Set(BusCacheKeys.Location(busId), new BusLocationModel(latitude, longitude, DateTime.UtcNow));
        return busId;
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
        var location = cache.Get<BusLocationModel>(BusCacheKeys.Location(bus.Id));
        return bus.ToModel(location);
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

    public async Task<GetBookingModel> AddBookingAsync(int lineId, int passengerId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        if (await unitOfWork.BookingRepository.HasActiveBookingOnLineAsync(passengerId, lineId, cancellationToken))
            throw new AlreadyExistsException(Phrases.AlreadyBooked);

        BookingEntity booking = BookingEntity.Create(lineId, passengerId, latitude, longitude);
        await unitOfWork.BookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return booking.ToModel();
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
