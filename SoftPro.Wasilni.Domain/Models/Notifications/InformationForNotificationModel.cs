namespace SoftPro.Wasilni.Domain.Models.Notifications;

public record InformationForNotificationModel(string FCMToken,int PassengerId,int TripId);
