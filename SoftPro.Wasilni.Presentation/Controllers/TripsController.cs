using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
[Authorize]
public class TripsController(ITripService tripService) : BaseController
{
    [HttpGet("my-active")]
    public async Task<GetTripResponse?> GetMyActiveTripAsync(CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        GetTripModel? model = await tripService.GetMyActiveTripAsync(driverId, cancellationToken);
        return model?.ToResponse();
    }
}
