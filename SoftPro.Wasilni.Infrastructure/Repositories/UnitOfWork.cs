using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public IAccountRepository        AccountRepository        { get; } = new AccountRepository(dbContext);
    public IBusRepository            BusRepository            { get; } = new BusRepository(dbContext);
    public ILineRepository           LineRepository           { get; } = new LineRepository(dbContext);
    public IBookingRepository        BookingRepository        { get; } = new BookingRepository(dbContext);
    public IDailyRidershipRepository DailyRidershipRepository { get; } = new DailyRidershipRepository(dbContext);

    public Task CompleteAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);

    public async Task Transaction(Func<Task> doTransacion, CancellationToken cancellation)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await doTransacion();
            await transaction.CommitAsync(cancellation);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellation);
            throw;
        }
    }
}
