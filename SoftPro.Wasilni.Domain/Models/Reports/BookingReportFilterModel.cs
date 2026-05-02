using SoftPro.Wasilni.Domain.Enums;

public record BookingReportFilterModel(
    ReportType Type,
    DateTime From,
    DateTime To,
    int? LineId,
    DateTime? BeginDateOfBirth,
    DateTime? EndDateOfBirth,
    Gender? Gender,
    BookingStatus? Status);