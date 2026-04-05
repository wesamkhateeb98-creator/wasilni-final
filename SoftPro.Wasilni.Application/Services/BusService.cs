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

    public async Task<GetActiveBusModel> ActivateBusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.Status == BusStatus.Active)
            throw new FailedPreconditionException(Phrases.BusAlreadyActive);

        if (bus.LineId is null)
            throw new FailedPreconditionException(Phrases.LineNotFound);

        bus.Activate();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Set(BusCacheKeys.DriverContext(driverId), new DriverContextCache(bus.Id, bus.LineId!.Value));

        return bus.ToModel(null);
    }

    public async Task<GetActiveBusModel> DeactivateBusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.Status != BusStatus.Active)
            throw new FailedPreconditionException(Phrases.BusNotOnRoad);

        bus.Deactivate();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Remove(BusCacheKeys.DriverContext(driverId));

        return bus.ToModel(null);
    }

    public async Task<UpdateLocationResult> UpdateLocationAsync(UpdateBusLocationModel model, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverContext(model.DriverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(model.DriverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(model.DriverId), ctx);
        }
        return new UpdateLocationResult(ctx.BusId, ctx.LineId);
    }

    public async Task<AdjustAnonymousResult> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverContext(driverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity cached = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (cached.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            ctx = new DriverContextCache(cached.Id, cached.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(driverId), ctx);
        }

        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(ctx.BusId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        bus.AdjustAnonymous(delta);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new AdjustAnonymousResult(bus.Id, ctx.LineId, bus.AnonymousCount);
    }

    public async Task<int> ConfirmRiderAsync(int driverId, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverContext(driverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(driverId), ctx);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await unitOfWork.DailyRidershipRepository.IncrementAsync(new IncrementRidershipModel(ctx.LineId, ctx.BusId, today), cancellationToken);
    }

    // ─── Driver: Bookings ─────────────────────────────────────────────────────

    public async Task<List<GetBookingModel>> GetBookingForLineAsync(int driverId, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverContext(driverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(driverId), ctx);
        }

        List<BookingEntity> bookings =
            await unitOfWork.BookingRepository.GetWaitingByLineAsync(ctx.LineId, cancellationToken);

        return bookings.Select(b => b.ToModel()).ToList();
    }

    public async Task<BookingActionResult> ConfirmBookingAsync(
        int bookingId, int driverId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        if (booking.Status != BookingStatus.Waiting)
            throw new FailedPreconditionException(Phrases.AlreadyBooked);

        if (!cache.TryGetValue(BusCacheKeys.DriverContext(driverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(driverId), ctx);
        }

        booking.MarkPickedUp();
        await unitOfWork.CompleteAsync(cancellationToken);

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        await unitOfWork.DailyRidershipRepository.IncrementAsync(new IncrementRidershipModel(booking.LineId, ctx.BusId, today), cancellationToken);

        return new BookingActionResult(booking.Id, booking.LineId);
    }

    public async Task<BookingActionResult> MarkNoShowAsync(
        int bookingId, int driverId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        if (booking.Status != BookingStatus.Waiting)
            throw new FailedPreconditionException(Phrases.AlreadyBooked);

        if (!cache.TryGetValue(BusCacheKeys.DriverContext(driverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(driverId), ctx);
        }

        if (booking.LineId != ctx.LineId)
            throw new ForbiddenException(Phrases.Forbidden);

        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);

        return new BookingActionResult(booking.Id, booking.LineId);
    }

    // ─── Passenger ────────────────────────────────────────────────────────────

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
