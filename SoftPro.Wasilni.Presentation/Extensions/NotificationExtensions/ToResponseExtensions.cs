using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Notifications;
using SoftPro.Wasilni.Presentation.Models.Response.Notification;


namespace SoftPro.Wasilni.Presentation.Extensions.NotificationExtensions;

public static class ToResponseExtensions
{
    public static SentNotificationModel ToResponse(this SentNotificationModel model)
        => new SentNotificationModel(model.Success, model.MessageId, model.ErrorMessage);

    // تحويل عنصر واحد
    public static GetNotificationsResponse ToResponse(this GetNotificationsExtendedModel model)
        => new(
            model.Id,
            model.Title,
            model.Body,
            model.SentAt,
            model.NotificationStatus,
            model.NotificationType
        );

    // تحويل صفحة
    public static Page<GetNotificationsResponse> ToResponse(this Page<GetNotificationsExtendedModel> notificationsPage)
        => new(
            notificationsPage.PageNumber,
            notificationsPage.PageSize,
            notificationsPage.TotalPages,
            notificationsPage.Content.Select(n => n.ToResponse()).ToList()
        );

}
