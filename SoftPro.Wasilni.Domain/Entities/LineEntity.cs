using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Domain.Entities;

public class LineEntity : IEntity
{
    public string      Name   { get; private set; } = string.Empty;
    public List<Point> Points { get; private set; } = [];
    public List<BusEntity> Buses { get; private set; } = [];

    private LineEntity() { }

    public static LineEntity Create(AddLineModel model)
        => new() { Name = model.Name, Points = model.Points };

    public void SetName(string name)             => Name   = name;
    public void SetPoints(List<Point> points)    => Points = points;
}
