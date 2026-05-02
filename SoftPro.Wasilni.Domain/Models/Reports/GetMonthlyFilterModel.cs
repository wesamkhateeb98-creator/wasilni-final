using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Reports;

public record GetMonthlyFilterModel(
    int FromYear, int FromMonth,
    int ToYear, int ToMonth,
    int? LineId,
    int? BusId,
    DateTime? BeginDateOfBirth,
    DateTime? EndDateOfBirth,
    Gender? Gender,
    BookingStatus? Status);