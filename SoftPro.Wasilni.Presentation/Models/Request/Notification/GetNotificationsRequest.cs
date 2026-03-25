using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Notification;

public record GetNotificationsRequest(NotificationTypeFilter NotificationTypeFilter,int PageSize,int PageNumber);
