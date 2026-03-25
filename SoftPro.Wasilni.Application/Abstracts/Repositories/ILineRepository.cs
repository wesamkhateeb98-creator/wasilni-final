using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface ILineRepository : IRepository<LineEntity>
{
    public LineEntity? GetLineByName(string name, CancellationToken cancellationToken);
    public Task<Page<GetLineModel>> GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken);
}
