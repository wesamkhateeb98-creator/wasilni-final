using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Domain.Models.Lines;

public record AddLineModel(string Name, List<RegisterPointModel> Points);
