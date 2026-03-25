using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Entities;

public class TripEntity : IEntity
{
    public int BusId { get; private set; }
    public int DriverId { get; private set; }
    public int LineId { get; private set; }
    public TripStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int AnonymousCount { get; private set; }

    public BusEntity Bus { get; private set; } = null!;

    private TripEntity() { }

    public static TripEntity Create(int busId, int driverId, int lineId)
        => new()
        {
            BusId = busId,
            DriverId = driverId,
            LineId = lineId,
            Status = TripStatus.Active,
            StartedAt = DateTime.UtcNow,
            AnonymousCount = 0
        };

    public void End()
    {
        Status = TripStatus.Ended;
        EndedAt = DateTime.UtcNow;
    }

    public void AdjustAnonymous(int delta)
        => AnonymousCount = Math.Max(0, AnonymousCount + delta);
}
