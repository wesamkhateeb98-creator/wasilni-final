using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Bus;

public record GetBusRequestsForDriverResponse(int RequestId, int OwnerId, string OwnerName, string OwnerPhonenumber, int BusId, BusType Type, string Plate,double percentDriver/*, RequestBusStatus RequestBusStatus*/);
