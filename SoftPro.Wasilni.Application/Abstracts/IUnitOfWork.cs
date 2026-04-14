using SoftPro.Wasilni.Application.Abstracts.Repositories;

namespace SoftPro.Wasilni.Application.Abstracts;

public interface IUnitOfWork
{
    IAccountRepository AccountRepository { get; }
    IBusRepository BusRepository { get; }
    ILineRepository LineRepository { get; }
    IBookingRepository BookingRepository { get; }
    IDailyRidershipRepository DailyRidershipRepository { get; }
    Task CompleteAsync(CancellationToken cancellationToken);
    Task Transaction(Func<Task> doTransacion, CancellationToken cancellation);
}
