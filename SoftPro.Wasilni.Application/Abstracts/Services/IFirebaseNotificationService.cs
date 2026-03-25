using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Notifications;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IFirebaseNotificationService
{
    public Task<List<SentNotificationModel>> SendNotificationTripAsync(string title, string body, NotificationType notificationType, TimeSpan notificationTime, int pageNumber, int pageSize, CancellationToken cancellationToken);
    public Task<SentNotificationModel> SendNotificationRequestAsync(int senderId, int recieverId, int busId, string fcmReciever, string title, string body, NotificationType notificationType, TimeSpan notificationTime, CancellationToken cancellationToken);
    public Task<Page<GetNotificationsExtendedModel>> GetByAccountIdFilter(GetNotificationsModel getNotificationsModel, CancellationToken cancellationToken);

}
