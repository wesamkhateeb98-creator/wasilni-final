namespace SoftPro.Wasilni.Domain.Entities;

public class DailyRidershipEntity : IEntity
{
    public int BusId { get; private set; }
    public int LineId { get; private set; }
    public DateOnly Day { get; private set; }
    public int NumberOfRiders { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public BusEntity Bus { get; private set; } = null!;

    private DailyRidershipEntity() { }

    public static DailyRidershipEntity Create(int busId, int lineId, DateOnly day)
        => new() { BusId = busId, LineId = lineId, Day = day, NumberOfRiders = 0 };

    public void IncrementRiders() => NumberOfRiders++;
}
