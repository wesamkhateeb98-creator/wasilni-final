using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
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

    public Task<List<BookingEntity>> GetWaitingByLineAsync(int lineId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .Where(b => b.LineId == lineId && b.Status == BookingStatus.Waiting)
            .ToListAsync(cancellationToken);
}
