using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Notifications;
using SoftPro.Wasilni.Infrastructure.Persistence;


namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext dbContext) : Repository<NotificationEntity>(dbContext), INotificationRepository
{
    public async Task<Page<GetNotificationsExtendedModel>> GetByUserIdFilter(GetNotificationsModel getNotificationsModel, CancellationToken cancellationToken)
    {
        var baseQuery = dbContext.Notifications.AsNoTracking()
        .Where(n => n.ReceiverId == getNotificationsModel.UserId);

        if (getNotificationsModel.NotificationTypeFilter != NotificationTypeFilter.All)
        {
            NotificationType type = getNotificationsModel.NotificationTypeFilter switch
            {
                NotificationTypeFilter.TripWillGo => NotificationType.TripWillGo,
                NotificationTypeFilter.SendRequestToDriver => NotificationType.SendRequestToDriver,
                NotificationTypeFilter.AcceptRequestFromDriver => NotificationType.AcceptRequestFromDriver,
                NotificationTypeFilter.DenyRequestFromDriver => NotificationType.DenyRequestFromDriver,
                _ => NotificationType.TripWillGo
            };
            baseQuery = baseQuery.Where(n => n.NotificationType == type);
        }

        // العدّ يتم على query أخف بدون projection ثقيل
        var count = await baseQuery
            .Select(n => n.Id) // تقليل الحمل
            .CountAsync(cancellationToken);

        var data = await baseQuery
            .OrderByDescending(n => n.SentAt) // الأحدث أولاً
            .Skip((getNotificationsModel.PageNumber - 1) * getNotificationsModel.PageSize)
            .Take(getNotificationsModel.PageSize)
            .Select(n => new GetNotificationsExtendedModel(
                n.Id,
                n.Title,
                n.Body,
                n.SentAt,
                n.Status,
                n.NotificationType
            ))
            .ToListAsync(cancellationToken);

        return new Page<GetNotificationsExtendedModel>(
            getNotificationsModel.PageNumber,
            getNotificationsModel.PageSize,
            (int)Math.Ceiling((double)count / getNotificationsModel.PageSize),
            data
        );
    }
}
