namespace SoftPro.Wasilni.Presentation.Models.Request.Line;

public record UpdateLineRequest(string Name, List<WayPointRequest> Points);
