using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IBookingRepository : IRepository<BookingEntity>
{
    /// <summary>Admin: paged bookings filtered by optional status and/or lineId, ordered by Date.</summary>
    Task<Page<GetAdminBookingModel>> GetBookingsForAdminAsync(GetAdminBookingsFilterModel filter, CancellationToken cancellationToken);

    /// <summary>Returns the passenger's single active (Waiting) booking, regardless of line.</summary>
    Task<BookingEntity?> GetActiveByPassengerAsync(int passengerId, CancellationToken cancellationToken);

    /// <summary>Returns the passenger's active booking with Line included (for display).</summary>
    Task<BookingEntity?> GetActiveByPassengerWithLineAsync(int passengerId, CancellationToken cancellationToken);

    /// <summary>Returns true if the passenger already has a Waiting booking on any line.</summary>
    Task<bool> HasActiveBookingAsync(int passengerId, CancellationToken cancellationToken);

    /// <summary>All <see cref="BookingStatus.Waiting"/> bookings on a given line, with passenger name.</summary>
    Task<List<GetBookingModel>> GetWaitingByLineAsync(int lineId, CancellationToken cancellationToken);
    Task<BookingEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);

    /// <summary>Returns true if the line has at least one Waiting booking.</summary>
    Task<bool> HasWaitingBookingsByLineAsync(int lineId, CancellationToken cancellationToken);
}
