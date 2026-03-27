using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IBookingRepository : IRepository<BookingEntity>
{
    Task<BookingEntity?> GetActiveByPassengerAndLineAsync(int passengerId, int lineId, CancellationToken cancellationToken);
    Task<bool> HasActiveBookingOnLineAsync(int passengerId, int lineId, CancellationToken cancellationToken);
}
