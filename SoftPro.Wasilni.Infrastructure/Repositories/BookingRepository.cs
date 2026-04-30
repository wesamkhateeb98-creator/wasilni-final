using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class BookingRepository(AppDbContext dbContext) : Repository<BookingEntity>(dbContext), IBookingRepository
{
    public Task<BookingEntity?> GetActiveByPassengerAsync(int passengerId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .FirstOrDefaultAsync(
                b => b.PassengerId == passengerId &&
                     b.Status      == BookingStatus.Waiting,
                cancellationToken);

    public Task<BookingEntity?> GetActiveByPassengerWithLineAsync(int passengerId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .Include(b => b.Line)
            .FirstOrDefaultAsync(
                b => b.PassengerId == passengerId &&
                     b.Status      == BookingStatus.Waiting,
                cancellationToken);

    public Task<bool> HasActiveBookingAsync(int passengerId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .AnyAsync(
                b => b.PassengerId == passengerId &&
                     b.Status      == BookingStatus.Waiting,
                cancellationToken);

    public Task<List<GetBookingModel>> GetWaitingByLineAsync(int lineId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .Where(b => b.LineId == lineId && b.Status == BookingStatus.Waiting)
            .Select(b => new GetBookingModel(
                b.Id,
                b.LineId,
                b.PassengerId,
                string.Join(" ", new[] { b.Passenger.FirstName, b.Passenger.LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value))),
                b.Date,
                b.Latitude,
                b.Longitude,
                b.Status,
                b.CreatedAt))
            .ToListAsync(cancellationToken);

    public Task<BookingEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken)
        => dbContext.Bookings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

    public Task<bool> HasWaitingBookingsByLineAsync(int lineId, CancellationToken cancellationToken)
        => dbContext.Bookings.AnyAsync(
            b => b.LineId == lineId && b.Status == BookingStatus.Waiting,
            cancellationToken);

    public async Task<Page<GetAdminBookingModel>> GetBookingsForAdminAsync(
        GetAdminBookingsFilterModel filter, CancellationToken cancellationToken)
    {
        IQueryable<BookingEntity> query = dbContext.Bookings
            .Where(b => filter.Status == null || b.Status == filter.Status)
            .Where(b => filter.LineId == null || b.LineId == filter.LineId)
            .OrderByDescending(b => b.Date);

        int count = await query.CountAsync(cancellationToken);

        List<GetAdminBookingModel> list = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => new GetAdminBookingModel(
                b.Id,
                b.PassengerId,
                string.Join(" ", new[] { b.Passenger.FirstName, b.Passenger.LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value))),
                b.LineId,
                b.Line.Name,
                b.Date,
                b.Latitude,
                b.Longitude,
                b.Status,
                b.CreatedAt))
            .ToListAsync(cancellationToken);

        return new(filter.PageNumber, filter.PageSize,
            (int)Math.Ceiling((double)count / filter.PageSize), list);
    }
}
