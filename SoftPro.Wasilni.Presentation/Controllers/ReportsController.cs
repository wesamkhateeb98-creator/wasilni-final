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
    [HttpGet("bus/{busId}")]
    public Task<List<RidershipReportItem>> GetByBusAsync(
        [FromRoute] int busId,
        [FromQuery] GetReportFilterRequest request,
        CancellationToken cancellationToken)
        => reportService.GetAsync(new GetReportFilterModel(request.Type, request.From, request.To, null, busId,request.BeginDateOfBirth, request.EndDateOfBirth, request.Gender), cancellationToken);

    [HttpGet("line/{lineId}")]
    public Task<List<RidershipReportItem>> GetByLineAsync(
        [FromRoute] int lineId,
        [FromQuery] GetReportFilterRequest request,
        CancellationToken cancellationToken)
        => reportService.GetAsync(new GetReportFilterModel(request.Type, request.From, request.To, lineId, null, request.BeginDateOfBirth, request.EndDateOfBirth, request.Gender), cancellationToken);
}
