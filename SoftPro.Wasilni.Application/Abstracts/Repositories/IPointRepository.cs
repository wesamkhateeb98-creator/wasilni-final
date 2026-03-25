using Microsoft.AspNetCore.Mvc.RazorPages;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IPointRepository : IRepository<PointEntity>
{
    Task<Page<GetPointsModel>> GetAllPoints(GetModelPaged input, CancellationToken cancellationToken);
    Task<List<GetPointsModel>> GetPointsByLineIdAsync(int lineId, CancellationToken cancellationToken);
    Task<PointEntity?> GetPointForLineAsync(int lineId, int pointId, CancellationToken cancellationToken);
}
