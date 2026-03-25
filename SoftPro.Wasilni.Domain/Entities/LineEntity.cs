using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Domain.Entities;

public class LineEntity : IEntity
{
    public string Name { get; private set; }
    public List<BusEntity> Buses { get; private set; } = [];

    private LineEntity(string name) => Name = name;

    public static LineEntity Create(AddLineModel model)
        => new(model.Name);

    public void SetName(string name) => Name = name;
}
