using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Presentation.Models.Request.Line;

public record AddLineRequest(string Name, List<RegisterPointModel> Points);
