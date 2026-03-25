using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Notification;

public record GetNotificationsResponse(int Id,string Title,string Body,DateTime SentAt,NotificationStatus NotificationStatus,NotificationType NotificationType);
