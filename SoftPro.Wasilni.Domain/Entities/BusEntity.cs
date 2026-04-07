using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Buses;

namespace SoftPro.Wasilni.Domain.Entities;

public class BusEntity : IEntity
{
    public string Plate { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public int? LineId { get; private set; }
    public BusType Type { get; private set; }
    public int NumberOfSeats { get; private set; }
    public int? OwnId { get; private set; }
    public int? DriverId { get; private set; }
    public BusStatus Status { get; private set; } = BusStatus.Inactive;
    public DateTime? ActiveSince { get; private set; }
    public int AnonymousCount { get; private set; }
    public Guid Key { get; private set; }

    public LineEntity? LineEntity { get; private set; }
    public AccountEntity? Own { get; private set; }
    public AccountEntity? Driver { get; private set; }

    private BusEntity() { }

    public static BusEntity Create(AddBusModel model)
        => new()
        {
            Plate = model.Plate,
            Color = model.Color,
            LineId = model.LineId,
            Type = model.Type,
            Status = BusStatus.Inactive,
            Key = model.key,
        };

    public void Update(UpdateBusModel model)
    {
        Plate = model.Plate;
        Color = model.Color;
        LineId = model.LineId;
        Type = model.Type;
    }

    public void AssignDriverId(int id) => DriverId = id;
    public void UnassignDriver() => DriverId = null;

    public void Activate()
    {
        Status = BusStatus.Active;
        ActiveSince = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = BusStatus.Inactive;
        ActiveSince = null;
        AnonymousCount = 0;
    }

    public void AdjustAnonymous(int delta)
        => AnonymousCount = Math.Max(0, AnonymousCount + delta);
}
