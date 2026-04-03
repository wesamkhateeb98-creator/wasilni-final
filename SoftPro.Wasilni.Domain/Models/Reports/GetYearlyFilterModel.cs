namespace SoftPro.Wasilni.Domain.Models.Reports;

public record GetYearlyFilterModel(int FromYear, int ToYear, int? LineId, int? BusId);
