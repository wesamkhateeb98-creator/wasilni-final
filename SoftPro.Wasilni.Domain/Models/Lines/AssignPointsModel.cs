using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Domain.Models.Lines;

public record AssignPointsModel(int LineId,List<RegisterPointModel> Points);
