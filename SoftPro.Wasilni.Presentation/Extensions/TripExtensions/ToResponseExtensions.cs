using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Models.Response.Bus;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Extensions.TripExtensions;

public static class ToResponseExtensions
{
    public static GetActiveBusResponse ToResponse(this GetActiveBusModel model)
        => new(model.BusId, model.Plate, model.LineId, model.LineName,
               model.Status.ToString(), model.Latitude, model.Longitude, model.ActiveSince);

    public static GetBookingResponse ToResponse(this GetBookingModel model)
        => new(model.Id, model.LineId, model.PassengerId, model.Date,
               model.Latitude, model.Longitude,
               model.Status.ToString(), model.CreatedAt);

    public static GetAdminBookingResponse ToResponse(this GetAdminBookingModel model)
        => new(model.BookingId, model.PassengerId, model.PassengerName, model.LineId, model.LineName,
               model.Date, model.Latitude, model.Longitude,
               model.Status.ToString(), model.CreatedAt);

    public static Page<GetAdminBookingResponse> ToResponse(this Page<GetAdminBookingModel> page)
        => new(page.PageNumber, page.PageSize, page.TotalPages,
               page.Content.Select(m => m.ToResponse()).ToList());
}
