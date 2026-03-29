using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Presentation.Models.Request.Report;
using SoftPro.Wasilni.Presentation.Models.Response.Report;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/reports")]
[Authorize(Roles = nameof(Role.Admin))]
public class ReportsController(IReportService reportService) : BaseController
{
    [HttpGet]
    public async Task<List<RidershipReportItem>> GetAsync(
        [FromQuery] GetReportRequest request,
        CancellationToken cancellationToken)
    {
        return await reportService.GetAsync(
            request.Type, request.From, request.To,
            request.LineId, cancellationToken);
    }
}
