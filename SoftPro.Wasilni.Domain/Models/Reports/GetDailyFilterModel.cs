namespace SoftPro.Wasilni.Domain.Models.Reports;

public record GetDailyFilterModel(DateOnly From, DateOnly To, int? LineId, int? BusId);
