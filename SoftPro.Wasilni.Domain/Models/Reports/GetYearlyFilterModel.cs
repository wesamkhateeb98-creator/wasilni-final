using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Reports;

public record GetYearlyFilterModel(
    int FromYear,
    int ToYear,
    int? LineId,
    int? BusId,
    DateTime? BeginDateOfBirth,
    DateTime? EndDateOfBirth,
    Gender? Gender,
    BookingStatus? Status);