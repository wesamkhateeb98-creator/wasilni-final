using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Entities;

public class BookingEntity : IEntity
{
    public int LineId { get; private set; }
    public int PassengerId { get; private set; }
    public DateOnly Date { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public LineEntity Line { get; private set; } = null!;
    public AccountEntity Passenger { get; private set; } = null!;

    private BookingEntity() { }

    public static BookingEntity Create(int lineId, int passengerId, double latitude, double longitude)
        => new()
        {
            LineId = lineId,
            PassengerId = passengerId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Latitude = latitude,
            Longitude = longitude,
            Status = BookingStatus.Waiting,
            CreatedAt = DateTime.UtcNow,
        };

    public void Cancel() => Status = BookingStatus.Cancelled;
    public void MarkPickedUp() => Status = BookingStatus.PickedUp;
}
