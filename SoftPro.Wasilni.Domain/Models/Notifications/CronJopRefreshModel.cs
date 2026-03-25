namespace SoftPro.Wasilni.Domain.Models.Notifications;

public record CronJopRefreshModel(TimeSpan NotificationTime, int PageNumber, int PageSize);
