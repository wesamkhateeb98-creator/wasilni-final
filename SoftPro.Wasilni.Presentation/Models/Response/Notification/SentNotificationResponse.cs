namespace SoftPro.Wasilni.Presentation.Models.Response.Notification;

public record SentNotificationResponse(bool Success, string? MessageId, string? ErrorMessage);
