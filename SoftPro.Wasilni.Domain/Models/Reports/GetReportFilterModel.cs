using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Reports;

public record GetReportFilterModel(
    ReportType Type,
    DateTime From,
    DateTime To,
    int? LineId,
    int? BusId,
    DateTime? BeginDateOfBirth,
    DateTime? EndDateOfBirth,
    Gender? Gender);