using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class PointRepository(AppDbContext dbContext) : Repository<PointEntity>(dbContext), IPointRepository
{
    public async Task<Page<GetPointsModel>> GetAllPoints(GetModelPaged input, CancellationToken cancellationToken)
    {
        IQueryable<PointEntity> points = dbContext.Points.AsNoTracking();

        int count = await points.CountAsync(cancellationToken);

        List<GetPointsModel> result =
        await points
        .Select(x => new GetPointsModel(x.Id, x.Latitude, x.Longitude, x.LineId))
        .Skip((input.PageNumber - 1)* input.PageSize)
        .Take(input.PageSize)
        .ToListAsync(cancellationToken);

        return new(
                input.PageNumber,
                input.PageSize,
                (int)Math.Ceiling((double)count / input.PageSize),
                result
            );
    }

    public async Task<List<GetPointsModel>> GetPointsByLineIdAsync(int lineId, CancellationToken cancellationToken)
        => await dbContext.Points
            .AsNoTracking()
            .Where(x => x.LineId == lineId)
            .Select(x => new GetPointsModel(x.Id, x.Latitude, x.Longitude, x.LineId))
            .ToListAsync(cancellationToken);

    public async Task<PointEntity?> GetPointForLineAsync(int lineId, int pointId, CancellationToken cancellationToken)
        => await dbContext.Points
            .FirstOrDefaultAsync(x => x.LineId == lineId && x.Id == pointId, cancellationToken);
}
