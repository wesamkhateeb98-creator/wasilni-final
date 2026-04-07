using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface ILineRepository : IRepository<LineEntity>
{
    LineEntity? GetLineByName(string name, CancellationToken cancellationToken);
    Task<Page<GetLineModel>> GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken);
    Task<LineEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);
}
