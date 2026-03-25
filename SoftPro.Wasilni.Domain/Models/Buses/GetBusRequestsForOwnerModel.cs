using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record GetBusRequestsForOwnerModel(int RequestId,int DriverId, string DriverName, string DriverPhonenumber, int BusId, BusType Type, string Plate, double percentOwner/*,RequestBusStatus RequestBusStatus*/);
