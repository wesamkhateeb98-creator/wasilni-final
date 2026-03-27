using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Entities;

public class BookingEntity : IEntity
{
    public int TripId { get; private set; }
    public int PassengerId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public TripEntity Trip { get; private set; } = null!;
    public AccountEntity Passenger { get; private set; } = null!;

    private BookingEntity() { }

    public static BookingEntity Create(int tripId, int passengerId, double latitude, double longitude)
        => new()
        {
            TripId      = tripId,
            PassengerId = passengerId,
            Latitude    = latitude,
            Longitude   = longitude,
            Status      = BookingStatus.Waiting,
            CreatedAt   = DateTime.UtcNow
        };

    public void Cancel()    => Status = BookingStatus.Cancelled;
    public void MarkPickedUp() => Status = BookingStatus.PickedUp;
}
