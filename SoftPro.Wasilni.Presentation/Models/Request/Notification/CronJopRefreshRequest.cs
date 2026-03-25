namespace SoftPro.Wasilni.Presentation.Models.Request.Notification;

public record CronJopRefreshRequest(TimeSpan NotificationTime,int PageNumber,int PageSize);
