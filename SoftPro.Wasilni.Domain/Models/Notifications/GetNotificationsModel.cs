using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Notifications;

public record GetNotificationsModel(int UserId,NotificationTypeFilter NotificationTypeFilter,int PageNumber,int PageSize);
