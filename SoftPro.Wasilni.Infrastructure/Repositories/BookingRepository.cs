using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class BookingRepository(AppDbContext dbContext) : Repository<BookingEntity>(dbContext), IBookingRepository
{
    public Task<BookingEntity?> GetActiveByPassengerAndTripAsync(int passengerId, int tripId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .FirstOrDefaultAsync(
                b => b.PassengerId == passengerId &&
                     b.TripId      == tripId      &&
                     b.Status      == BookingStatus.Waiting,
                cancellationToken);

    public Task<bool> HasActiveBookingOnTripAsync(int passengerId, int tripId, CancellationToken cancellationToken)
        => dbContext.Bookings
            .AnyAsync(
                b => b.PassengerId == passengerId &&
                     b.TripId      == tripId      &&
                     b.Status      == BookingStatus.Waiting,
                cancellationToken);
}
