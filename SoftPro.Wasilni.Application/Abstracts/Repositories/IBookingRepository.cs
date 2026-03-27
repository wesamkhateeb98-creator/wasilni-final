using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IBookingRepository : IRepository<BookingEntity>
{
    Task<BookingEntity?> GetActiveByPassengerAndTripAsync(int passengerId, int tripId, CancellationToken cancellationToken);
    Task<bool> HasActiveBookingOnTripAsync(int passengerId, int tripId, CancellationToken cancellationToken);
}
