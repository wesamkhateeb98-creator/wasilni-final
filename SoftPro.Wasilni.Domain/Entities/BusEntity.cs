using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Buses;

namespace SoftPro.Wasilni.Domain.Entities;

public class BusEntity : IEntity
{
    public BusEntity(
        string plate, 
        string color, 
        int lineId,
        BusType type, 
        int numberOfSeats,
        int ownId, 
        int? driverId 
        )
    {
        Plate = plate;
        Color = color;
        LineId = lineId;
        Type = type;
        NumberOfSeats = numberOfSeats;
        OwnId = ownId;
        DriverId = driverId;
    }

    public string Plate { get; private set; }
    public string Color { get; private set; }
    public int LineId { get; private set; }
    public LineEntity LineEntity { get; private set; } = null!;
    public BusType Type { get; private set; }
    public int NumberOfSeats { get; private set; }

    public int OwnId { get; private set; }
    public AccountEntity Own { get; private set; } = null!;

    public int? DriverId { get; private set; }
    public AccountEntity Driver { get; private set; } = null!;

    public List<int>? RequestsBusIds { get; private set; } = [];
    //public List<RequestBusEntity> Requests { get; private set; } = [];

    public List<int>? TripIds { get; private set; } = [];
    //public List<TripEntity>? Trips { get; private set; } = [];
    public static BusEntity Create(RegisterBusModel model)
        => new(
            model.Plate,
            model.Color,
            model.lineId,
            model.Type,
            model.NumberOfSeats,
            model.accountId,
            null);

    public void Update(UpdateBusModel model)
    {
        Plate = model.Plate;
        Color = model.Color;
        LineId = model.LineId;
        Type = model.Type;
        NumberOfSeats = model.NumberOfSeats;
    }

    public void UnassignDriver()
        => DriverId = null;

    public void AssignDriverId(int id)
        => DriverId = id;

    
    public void AddTrip(int id)
    {
        TripIds?.Add(id);
    }
}
