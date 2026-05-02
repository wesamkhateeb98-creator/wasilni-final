// Presentation/Controllers/ReportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Presentation.Models.Request.Report;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/reports")]
[Authorize(Roles = nameof(Role.Admin))]
public class ReportsController(IReportService reportService) : BaseController
{
    // GET api/v1.0/reports/bus/5/bookings
    [HttpGet("bus/{busId}/bookings")]
    public Task<List<RidershipReportItem>> GetBusBookingsAsync(
        [FromRoute] int busId,
        [FromQuery] GetBookingReportRequest request,
        CancellationToken cancellationToken)
        => reportService.GetFromBookingsAsync(
            new BookingReportFilterModel(
                request.Type, request.From, request.To,
                LineId: null, // BookingEntity has no BusId
                request.BeginDateOfBirth, request.EndDateOfBirth,
                request.Gender, request.Status),
            cancellationToken);

    // GET api/v1.0/reports/bus/5/ridership
    [HttpGet("bus/{busId}/ridership")]
    public Task<List<RidershipReportItem>> GetBusRidershipAsync(
        [FromRoute] int busId,
        [FromQuery] GetRidershipReportRequest request,
        CancellationToken cancellationToken)
        => reportService.GetFromRidershipAsync(
            new RidershipReportFilterModel(
                request.Type, request.From, request.To,
                LineId: null, BusId: busId),
            cancellationToken);

    // GET api/v1.0/reports/line/3/bookings
    [HttpGet("line/{lineId}/bookings")]
    public Task<List<RidershipReportItem>> GetLineBookingsAsync(
        [FromRoute] int lineId,
        [FromQuery] GetBookingReportRequest request,
        CancellationToken cancellationToken)
        => reportService.GetFromBookingsAsync(
            new BookingReportFilterModel(
                request.Type, request.From, request.To,
                LineId: lineId,
                request.BeginDateOfBirth, request.EndDateOfBirth,
                request.Gender, request.Status),
            cancellationToken);

    // GET api/v1.0/reports/line/3/ridership
    [HttpGet("line/{lineId}/ridership")]
    public Task<List<RidershipReportItem>> GetLineRidershipAsync(
        [FromRoute] int lineId,
        [FromQuery] GetRidershipReportRequest request,
        CancellationToken cancellationToken)
        => reportService.GetFromRidershipAsync(
            new RidershipReportFilterModel(
                request.Type, request.From, request.To,
                LineId: lineId, BusId: null),
            cancellationToken);
}