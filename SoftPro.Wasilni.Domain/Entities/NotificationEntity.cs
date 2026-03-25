using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Entities;

public class NotificationEntity : IEntity
{
    public NotificationEntity(int receiverId, int tripId, string title, string body, DateTime sentAt, NotificationStatus status, NotificationType notificationType, string? errorMessage)
    {
        ReceiverId = receiverId;
        TripId = tripId;
        Title = title;
        Body = body;
        SentAt = sentAt;
        Status = status;
        ErrorMessage = errorMessage;
        NotificationType  = notificationType;
    }

    public NotificationEntity(NotificationType notificationType, int senderId, int receiverId, string title, string body, DateTime sentAt, NotificationStatus status, string? errorMessage)
    {
        NotificationType = notificationType;
        SenderId = senderId;
        ReceiverId = receiverId;
        Title = title;
        Body = body;
        SentAt = sentAt;
        Status = status;
        ErrorMessage = errorMessage;
    }

    public NotificationType NotificationType { get; private set; }
    public int SenderId { get; private set; }
    public int ReceiverId { get; private set; }
    public int? TripId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public DateTime SentAt { get; private set; }
    public NotificationStatus Status { get; private set; } = NotificationStatus.pendind; // Pending / Success / Failed
    public string? ErrorMessage { get; private set; }

    public AccountEntity Receiver { get; private set; } = null!;
    public AccountEntity? Sender { get; private set; }
    //public TripEntity? Trip { get; private set; } = null!;

    public void SetStatus(NotificationStatus status)
        => Status = status;

    public void SetErrorMessage(string message) 
        => ErrorMessage = message;
}
