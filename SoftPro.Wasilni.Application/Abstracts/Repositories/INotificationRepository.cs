using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Notifications;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface INotificationRepository : IRepository<NotificationEntity>
{
    public Task<Page<GetNotificationsExtendedModel>> GetByUserIdFilter(GetNotificationsModel getNotificationsModel, CancellationToken cancellationToken);
}
