using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Extensions.TripExtensions;

public static class ToResponseExtensions
{
    public static GetTripResponse ToResponse(this GetTripModel model)
        => new(model.Id, model.BusId, model.BusPlate, model.LineId, model.LineName,
               model.Status.ToString(), model.Latitude, model.Longitude,
               model.AnonymousCount, model.StartedAt);

    public static GetBookingResponse ToResponse(this GetBookingModel model)
        => new(model.Id, model.TripId, model.PassengerId,
               model.Latitude, model.Longitude,
               model.Status.ToString(), model.CreatedAt);
}
