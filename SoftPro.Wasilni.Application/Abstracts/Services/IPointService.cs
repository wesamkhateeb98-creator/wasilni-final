using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;


namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IPointService
{
    Task<Page<GetPointsModel>> GetPointsForAdminAsync(GetModelPaged input, CancellationToken cancellationToken);
    Task<int> RegisterAsync(RegisterPointModel registerModel, CancellationToken cancellationToken);
    Task<int> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<GetPointsModel> UpdatePointAsync(UpdatePointModel input, CancellationToken cancellationToken);
    Task<List<GetPointsModel>> GetLinePointsAsync(int lineId, CancellationToken cancellationToken);
    Task<GetPointsModel> UpdateLinePointAsync(UpdateLinePointModel model, CancellationToken cancellationToken);
    Task<int> AddLinePointAsync(AddLinePointModel model, CancellationToken cancellationToken);
}
