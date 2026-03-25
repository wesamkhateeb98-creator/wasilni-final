using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Extensions.TripExtensions;

public static class ToResponseExtensions
{
    public static GetTripResponse ToResponse(this GetTripModel model)
        => new(model.Id, model.BusId, model.BusPlate, model.LineId, model.LineName,
               model.Status.ToString(), model.Latitude, model.Longitude,
               model.AnonymousCount, model.StartedAt);
}
