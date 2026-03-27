using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;



namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface ILineService
{
    public Task<int> AddLine(AddLineModel model, CancellationToken cancellationToken);
    public Task<int> UpdateLine(GetLineModel model, CancellationToken cancellationToken);
    public Task<int> DeleteLine(int id, CancellationToken cancellationToken);
    public Task<Page<GetLineModel>> GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken);
}
