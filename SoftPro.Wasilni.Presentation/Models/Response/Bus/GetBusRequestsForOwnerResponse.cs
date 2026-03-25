using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Bus;

public record GetBusRequestsForOwnerResponse(int RequestId, int DriverId, string DriverName, string DriverPhonenumber, int BusId, BusType Type, string Plate, double percentOwner/*, RequestBusStatus RequestBusStatus*/);
