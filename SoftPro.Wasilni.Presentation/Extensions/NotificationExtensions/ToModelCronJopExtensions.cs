using SoftPro.Wasilni.Domain.Models.Notifications;
using SoftPro.Wasilni.Presentation.Models.Request.Notification;


namespace SoftPro.Wasilni.Presentation.Extensions.NotificationExtensions;

public static class ToModelCronJopExtensions
{
    public static CronJopRefreshModel ToModel(this CronJopRefreshRequest cronJopRefreshRequest)
        => new(cronJopRefreshRequest.NotificationTime, cronJopRefreshRequest.PageNumber, cronJopRefreshRequest.PageSize);

    public static GetNotificationsModel ToModel(this GetNotificationsRequest getNotificationsRequest,int UserId)
        => new GetNotificationsModel(UserId,getNotificationsRequest.NotificationTypeFilter, getNotificationsRequest.PageNumber, getNotificationsRequest.PageSize);
}
