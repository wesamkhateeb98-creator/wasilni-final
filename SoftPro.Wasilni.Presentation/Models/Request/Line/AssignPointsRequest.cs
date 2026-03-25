using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Presentation.Models.Request.Line;

public record AssignPointsRequest(List<RegisterPointModel> Points);
