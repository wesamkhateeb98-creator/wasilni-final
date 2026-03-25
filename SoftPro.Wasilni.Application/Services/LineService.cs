using Domain.Resources;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;

namespace SoftPro.Wasilni.Application.Services;

public class LineService(IUnitOfWork unitOfWork) : ILineService
{
    public async Task<int> AddLine(AddLineModel model, CancellationToken cancellationToken)
    {
        LineEntity? oldLine = unitOfWork.LineRepository.GetLineByName(model.Name, cancellationToken);

        if (oldLine != null) throw new AlreadyExistsException(Phrases.LineAlreadyExists);

        LineEntity line = LineEntity.Create(model);

        await unitOfWork.Transaction(async () =>
        {
            await unitOfWork.LineRepository.AddAsync(line, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            List<PointEntity> points = [.. model.Points.Select(p => PointEntity.Create(p, line.Id))];

            await unitOfWork.PointRepository.AddAllAsync(points, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

        }, cancellationToken);

        return line.Id;
    }

    public async Task<int> DeleteLine(int id, CancellationToken cancellationToken)
    {
        LineEntity? oldLine = await unitOfWork.LineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AlreadyExistsException(Phrases.LineNotFound);

        unitOfWork.LineRepository.Delete(oldLine, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return id;
    }

    public async Task<GetLineModel> UpdateLine(GetLineModel model, CancellationToken cancellationToken)
    {
        LineEntity? line = await unitOfWork.LineRepository.GetByIdAsync(model.Id, cancellationToken);

        if (line is null) throw new NotFoundException(Phrases.LineNotFound);

        line.SetName(model.Name);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new(line.Id, line.Name);
    }

    public Task<Page<GetLineModel>> GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken)
        => unitOfWork.LineRepository.GetLinesAsync(filter, cancellationToken);
}
