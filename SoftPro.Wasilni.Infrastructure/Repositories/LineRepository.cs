using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class LineRepository(AppDbContext dbContext) : Repository<LineEntity>(dbContext), ILineRepository
{
    public Task<LineEntity?> GetLineByName(string name, CancellationToken cancellationToken)
        => dbContext.Lines.FirstOrDefaultAsync(x => x.Name == name);

    public async Task<Page<GetLineModel>> GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken)
    {
        IQueryable<LineEntity> query = dbContext.Lines.AsQueryable();

        if (filter.Name is not null)
            query = query.Where(x => x.Name.Contains(filter.Name));

        int count = await query.CountAsync(cancellationToken);

        List<GetLineModel> list = await query
            .Select(x => new GetLineModel(x.Id, x.Name))
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new(filter.PageNumber, filter.PageSize, (int)Math.Ceiling((double)count / filter.PageSize), list);
    }

    public Task<LineEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken)
        => dbContext.Lines.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
}
