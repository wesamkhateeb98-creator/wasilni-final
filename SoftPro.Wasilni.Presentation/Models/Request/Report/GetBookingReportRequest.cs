using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Report;

public record GetBookingReportRequest(
    ReportType Type,
    DateTime From,
    DateTime To,
    DateTime? BeginDateOfBirth,
    DateTime? EndDateOfBirth,
    Gender? Gender,
    BookingStatus? Status);