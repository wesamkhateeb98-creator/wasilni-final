namespace SoftPro.Wasilni.Domain.Models.Notifications;

public record SentNotificationModel(bool Success, string? MessageId, string? ErrorMessage);
