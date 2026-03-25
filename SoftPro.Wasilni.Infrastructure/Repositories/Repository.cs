using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Infrastructure.Persistence;


namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class Repository<T>(AppDbContext dbContext) : IRepository<T> where T : IEntity
{
    public async Task AddAsync(T t, CancellationToken cancellationToken)
        => await dbContext.Set<T>().AddAsync(t, cancellationToken);

    public async Task AddAllAsync(List<T> entities, CancellationToken cancellationToken)
    => await dbContext.Set<T>().AddRangeAsync(entities, cancellationToken);

    public Task<bool> AnyAsync(int id, CancellationToken cancellationToken)
        => dbContext.Set<T>().AnyAsync(x => x.Id == id, cancellationToken);

    public void Delete(T t, CancellationToken cancellationToken)
        => dbContext.Set<T>().Remove(t);

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<bool> ExistsAllAsync(List<int> ids, CancellationToken cancellation)
    {
        int count = await dbContext.Set<T>().Where(x => ids.Contains(x.Id)).CountAsync(cancellation);
        return ids.Count == count;
    }

    public Task<List<T>> GetByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        => dbContext.Set<T>().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);


}
