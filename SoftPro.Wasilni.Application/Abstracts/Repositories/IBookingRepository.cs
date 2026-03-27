using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IBookingRepository : IRepository<BookingEntity>
{
    Task<BookingEntity?> GetActiveByPassengerAndLineAsync(int passengerId, int lineId, CancellationToken cancellationToken);
    Task<bool>           HasActiveBookingOnLineAsync(int passengerId, int lineId, CancellationToken cancellationToken);

    /// <summary>All <see cref="BookingStatus.Waiting"/> bookings on a given line.</summary>
    Task<List<BookingEntity>> GetWaitingByLineAsync(int lineId, CancellationToken cancellationToken);
}
