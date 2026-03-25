using FirebaseAdmin.Messaging;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Notifications;


namespace SoftPro.Wasilni.Application.Services;

public class FirebaseNotificationService(IUnitOfWork unitOfWork) : IFirebaseNotificationService
{
    public Task<Page<GetNotificationsExtendedModel>> GetByAccountIdFilter(GetNotificationsModel getNotificationsModel, CancellationToken cancellationToken)
        => unitOfWork.NotificationRepository.GetByUserIdFilter(getNotificationsModel, cancellationToken);

    public async Task<SentNotificationModel> SendNotificationRequestAsync(int senderId,int recieverId,int busId,string fcmReciever,string title, string body, NotificationType notificationType, TimeSpan notificationTime, CancellationToken cancellationToken)
    {

        NotificationEntity notificationEntity = new(notificationType, senderId, recieverId, title, body, DateTime.Now.AddHours(3), NotificationStatus.pendind, null);

        string notificationTypeStr = notificationType.ToString();
        
        SentNotificationModel data ;

        try
        {
            var message = new Message()
            {

                Token = fcmReciever,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>()
                    {
                        { "type", notificationTypeStr },
                        { "busId", busId.ToString() }
                    }
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message,cancellationToken);
        
            data = new SentNotificationModel(true, response, null);

            notificationEntity.SetStatus(NotificationStatus.success);

        }
        catch (Exception ex)
        {

            data = new SentNotificationModel(false, null, ex.Message);

            notificationEntity.SetStatus(NotificationStatus.failed);
        
            notificationEntity.SetErrorMessage(ex.Message);
        }
        await unitOfWork.NotificationRepository.AddAsync(notificationEntity,cancellationToken);
        return data;
    }

    public async Task<List<SentNotificationModel>> SendNotificationTripAsync(string title, string body,NotificationType notificationType, TimeSpan notificationTime,int pageNumber,int pageSize, CancellationToken cancellationToken)
    {
        return new();
        //List<InformationForNotificationModel> preData = await unitOfWork.TripRepository.GetInformationsForNotifications(notificationTime,pageNumber,pageSize,cancellationToken);

        //List<SentNotificationModel> data = [];
        
        //List<NotificationEntity> notifications = [];
      
        //foreach (InformationForNotificationModel p in preData)
        //{
        //    NotificationEntity notificationEntity = new(p.PassengerId, p.TripId, title, body, DateTime.Now.AddHours(3), NotificationStatus.pendind, notificationType, null);

        //    string notificationTypeStr = notificationType.ToString();

        //    try
        //    {
        //        var message = new Message()
        //        {

        //            Token = p.FCMToken,
        //            Notification = new Notification
        //            {
        //                Title = title,
        //                Body = body
        //            },
        //            Data = new Dictionary<string, string>()
        //            {
        //                { "type", notificationTypeStr },
        //                { "tripId", p.TripId.ToString() },
        //                { "passengerId", p.PassengerId.ToString() }
        //            }
        //        };

        //        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message,cancellationToken);

        //        notificationEntity.SetStatus(NotificationStatus.success);

        //        notifications.Add(notificationEntity);

        //        data.Add(new SentNotificationModel(true, response, null));
        //    }
        //    catch (Exception ex)
        //    {
        //        notificationEntity.SetStatus(NotificationStatus.failed);

        //        notificationEntity.SetErrorMessage(ex.Message);

        //        notifications.Add(notificationEntity );

        //        data.Add(new SentNotificationModel(false, null, ex.Message));
        //    }
        //}

        //await unitOfWork.NotificationRepository.AddAllAsync(notifications, cancellationToken);

        //await unitOfWork.CompleteAsync(cancellationToken);

        //return data;
    }
}
