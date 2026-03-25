namespace SoftPro.Wasilni.Presentation.Models.Request.Line;

public record GetLinesRequest(int PageNumber, int PageSize, string? Name);
