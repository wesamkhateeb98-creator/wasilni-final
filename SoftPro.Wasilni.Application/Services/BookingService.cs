using Domain.Resources;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Services;

public class BookingService(IUnitOfWork unitOfWork, IMemoryCache cache) : IBookingService
{
    // ─── Admin ────────────────────────────────────────────────────────────────

    public Task<Page<GetAdminBookingModel>> GetBookingsForAdminAsync(
        GetAdminBookingsFilterModel filter, CancellationToken cancellationToken)
        => unitOfWork.BookingRepository.GetBookingsForAdminAsync(filter, cancellationToken);

    // ─── Driver ───────────────────────────────────────────────────────────────

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

        return await unitOfWork.BookingRepository.GetWaitingByLineAsync(ctx.LineId, cancellationToken);
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

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get or create
        var ridership = await unitOfWork.DailyRidershipRepository
            .GetOrCreateAsync(new IncrementRidershipModel(booking.LineId, ctx.BusId, today), cancellationToken);

        if (ridership is null)
        {
            ridership = DailyRidershipEntity.Create(booking.LineId, ctx.BusId, today);
            await unitOfWork.DailyRidershipRepository.AddAsync(ridership, cancellationToken);
        }

        // Increment
        booking.MarkPickedUp();
        ridership.IncrementRiders();

        // Complete
        await unitOfWork.CompleteAsync(cancellationToken);

        return new BookingActionResult(booking.Id, booking.LineId, booking.PassengerId);
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

        return new BookingActionResult(booking.Id, booking.LineId, booking.PassengerId);
    }

    // ─── Passenger ────────────────────────────────────────────────────────────

    public async Task<MyBookingResult?> GetMyBookingAsync(int passengerId, CancellationToken cancellationToken)
    {
        BookingEntity? booking = await unitOfWork.BookingRepository
            .GetActiveByPassengerWithLineAsync(passengerId, cancellationToken);

        if (booking is null) return null;
        return new MyBookingResult(booking.Id, booking.LineId, booking.Line.Name);
    }

    public async Task<AddBookingResult> AddBookingAsync(CreateBookingModel model, CancellationToken cancellationToken)
    {
        BookingEntity? book = await unitOfWork.BookingRepository.FindByIdempotencyKeyAsync(model.key, cancellationToken);
        if (book is not null)
            throw new AlreadyExistsException(Phrases.AlreadyBooked);

        if (!await unitOfWork.LineRepository.AnyAsync(model.LineId, cancellationToken))
            throw new NotFoundException(Phrases.LineNotFound);

        if (await unitOfWork.BookingRepository.HasActiveBookingAsync(model.PassengerId, cancellationToken))
            throw new AlreadyExistsException(Phrases.AlreadyBooked);

        var passenger = await unitOfWork.AccountRepository.GetByIdAsync(model.PassengerId, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        BookingEntity booking = BookingEntity.Create(model.LineId, model.PassengerId, model.Latitude, model.Longitude, model.key);
        await unitOfWork.BookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new AddBookingResult(booking.Id, passenger.Name);
    }

    public async Task<BookingActionResult> CancelBookingAsync(int passengerId, CancellationToken cancellationToken)
    {
        BookingEntity booking = await unitOfWork.BookingRepository
            .GetActiveByPassengerAsync(passengerId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BookingNotFound);

        int lineId = booking.LineId;
        booking.Cancel();
        await unitOfWork.CompleteAsync(cancellationToken);

        return new BookingActionResult(booking.Id, lineId, passengerId);
    }
}
