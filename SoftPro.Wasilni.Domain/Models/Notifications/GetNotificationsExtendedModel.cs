using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Notifications;

public record GetNotificationsExtendedModel(int Id, string Title, string Body, DateTime SentAt, NotificationStatus NotificationStatus, NotificationType NotificationType);

