using SoftPro.Wasilni.Application.Abstracts.Repositories;

namespace SoftPro.Wasilni.Application.Abstracts;

public interface IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IBusRepository BusRepository { get; }
    INotificationRepository NotificationRepository { get; }
    IPointRepository PointRepository { get; }

    ILineRepository LineRepository { get; }
    Task CompleteAsync(CancellationToken cancellationToken);
    Task Transaction(Func<Task> doTransacion, CancellationToken cancellation);
}