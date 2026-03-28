using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface ILineService
{
    Task<int>          AddLineAsync(AddLineModel model, CancellationToken cancellationToken);
    Task<int>          UpdateLineNameAsync(int id, string name, CancellationToken cancellationToken);
    Task<int>          UpdateLinePointsAsync(int id, List<Point> points, CancellationToken cancellationToken);
    Task<int>          DeleteLineAsync(int id, CancellationToken cancellationToken);
    Task<Page<GetLineModel>>  GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken);
    Task<List<Point>>      GetLinePointsAsync(int id, CancellationToken cancellationToken);
}
