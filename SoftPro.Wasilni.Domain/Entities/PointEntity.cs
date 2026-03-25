using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Domain.Entities;

public class PointEntity : IEntity
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? LineId { get; set; }
    public LineEntity? Line { get; set; }

    private PointEntity(double latitude, double longitude, int? lineId)
    {
        Latitude = latitude;
        Longitude = longitude;
        LineId = lineId;
    }

    public static PointEntity Create(RegisterPointModel model, int? lineId = null)
        => new(model.Latitude, model.Longitude, lineId);

    public void Update(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
