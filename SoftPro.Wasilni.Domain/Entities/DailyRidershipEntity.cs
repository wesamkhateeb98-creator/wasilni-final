namespace SoftPro.Wasilni.Domain.Entities;

public class DailyRidershipEntity : IEntity
{
    public int LineId { get; private set; }
    public int BusId { get; private set; }
    public DateOnly Day { get; private set; }
    public int NumberOfRiders { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public LineEntity Line { get; private set; } = null!;
    public BusEntity Bus { get; private set; } = null!;

    private DailyRidershipEntity() { }

    public static DailyRidershipEntity Create(int lineId, int busId, DateOnly day)
        => new() { LineId = lineId, BusId = busId, Day = day, NumberOfRiders = 0 };

    public void IncrementRiders() => NumberOfRiders++;
    public void AdjustRiders(int delta) => NumberOfRiders = Math.Max(0, NumberOfRiders + delta);
}
