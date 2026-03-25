using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Notifications;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.NotificationExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Notification;
using SoftPro.Wasilni.Presentation.Models.Response.Notification;


namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
public class NotificationsController(IFirebaseNotificationService firebaseNotificationService) : BaseController
{
    [HttpPost("cron-job/refresh")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<List<SentNotificationModel>> CronJopRefresh([FromBody] CronJopRefreshRequest cronJopRefreshRequest,CancellationToken cancellationToken)
    {
        CronJopRefreshModel model = cronJopRefreshRequest.ToModel();
        
        //return await firebaseNotificationService.SendNotificationTripAsync(Title.TripWillGo, Phrases.TripWillGo,NotificationType.TripWillGo,model.NotificationTime,model.PageNumber,model.PageSize,cancellationToken);
        return await firebaseNotificationService.SendNotificationTripAsync("الرحلة ستمضي", "الرحلة ستتحرك في أقل من 15 دقيقة", NotificationType.TripWillGo, model.NotificationTime, model.PageNumber, model.PageSize, cancellationToken);
    }

    [HttpGet]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<Page<GetNotificationsResponse>> GetNotifications([FromQuery] GetNotificationsRequest getNotificationRequest, CancellationToken cancellationToken)
    {
        Page<GetNotificationsExtendedModel> data = await firebaseNotificationService.GetByAccountIdFilter(getNotificationRequest.ToModel(User.GetId()), cancellationToken);
       
        return data.ToResponse();
    }

}
