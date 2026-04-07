namespace SoftPro.Wasilni.Presentation.Models.Request.Line;

public record AddLineRequest(string Name, List<WayPointRequest> Points, Guid key);
