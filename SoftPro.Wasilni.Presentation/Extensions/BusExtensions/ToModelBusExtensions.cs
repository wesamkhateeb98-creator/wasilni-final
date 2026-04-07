using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;
using SoftPro.Wasilni.Presentation.Models.Response.Bus;


namespace SoftPro.Wasilni.Presentation.Extensions.BusExtensions;

public static class ToModelBusExtensions
{
    public static AddBusModel ToModel(this AddBusRequest request)
        => new(request.Plate, request.Color, request.LineId, request.Type, request.key);

    public static UpdateBusModel ToModel(this UpdateBusRequest request)
        => new(request.Plate, request.Color, request.LineId, request.Type);

    public static GetBusModel ToInput(this GetBusesRequest request, int id)
        => new(id, request.PageNumber, request.PageSize, request.Filter);

    public static GetBusForAdminModel ToInput(this GetBusesForAdminRequest request)
        => new(request.OwnerId, request.Plate, request.Filter, request.PageNumber, request.PageSize);

    public static Page<GetBusesResponse> ToResponse(this Page<GetBusesModel> model)
        => new(
                model.PageNumber,
                model.PageSize,
                model.TotalPages,
                model.Content.Select(x => x.ToResponse()).ToList()
            );

    public static GetBusesResponse ToResponse(this GetBusesModel model)
        => new(
            model.BusId,
            model.Owner,
            model.Plate,
            model.Color,
            model.Type,
            model.Line is not null ? new LineBusResponse(model.Line.Id, model.Line.Name) : null,
            model.Driver
            );

    public static Page<GetBusesForAdminResponse> ToResponse(this Page<GetBusesForAdminModel> model)
        => new(
                model.PageNumber,
                model.PageSize,
                model.TotalPages,
                model.Content.Select(x => x.ToResponse()).ToList()
            );

    public static GetBusesForAdminResponse ToResponse(this GetBusesForAdminModel model)
        => new(
            model.BusId,
            model.Plate,
            model.Color,
            model.Type,
            model.NumberOfSeats,
            model.LineId,
            model.Driver
            );

    public static LineBusResponse ToResponse(this LineBusModel model)
        => new(
            model.Id,
            model.Name
            );
    public static Page<GetBusRequestsForDriverResponse> ToResponse(this Page<GetBusRequestsForDriverModel> model)
        => new
        (
            PageNumber: model.PageNumber,
            PageSize: model.PageSize,
            TotalPages: model.TotalPages,
            Content: model.Content
                .Select(x =>
                    new GetBusRequestsForDriverResponse(
                        x.RequestId,x.OwnerId, x.OwnerName, x.OwnerPhonenumber, x.BusId, x.Type, x.Plate,x.percentDriver/*,x.RequestBusStatus*/
                        )).ToList()
        );

    public static Page<GetBusRequestsForOwnerResponse> ToResponse(this Page<GetBusRequestsForOwnerModel> model)
        => new
        (
            PageNumber: model.PageNumber,
            PageSize: model.PageSize,
            TotalPages: model.TotalPages,
            Content: model.Content
                .Select(x =>
                    new GetBusRequestsForOwnerResponse(
                        x.RequestId,x.DriverId, x.DriverName, x.DriverPhonenumber, x.BusId, x.Type, x.Plate,x.percentOwner/*,x.RequestBusStatus*/
                        )).ToList()
        );

}
