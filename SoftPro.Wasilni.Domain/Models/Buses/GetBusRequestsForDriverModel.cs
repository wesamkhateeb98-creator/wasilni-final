using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record GetBusRequestsForDriverModel(int RequestId,int OwnerId, string OwnerName, string OwnerPhonenumber, int BusId, BusType Type, string Plate, double percentDriver/*, RequestBusStatus RequestBusStatus*/);
