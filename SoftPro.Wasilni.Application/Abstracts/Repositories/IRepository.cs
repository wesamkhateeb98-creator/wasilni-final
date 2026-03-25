namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IRepository<T>
{
    Task AddAsync(T t, CancellationToken cancellationToken);
    Task AddAllAsync(List<T> entities, CancellationToken cancellationToken);
    void Delete(T t, CancellationToken cancellationToken);
    Task<bool> AnyAsync(int id, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAllAsync(List<int> ids, CancellationToken cancellation);
    Task<List<T>> GetByIdsAsync(List<int> ids, CancellationToken cancellationToken);

}