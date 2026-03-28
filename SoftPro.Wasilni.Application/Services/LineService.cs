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
    public async Task<int> AddLineAsync(AddLineModel model, CancellationToken cancellationToken)
    {
        LineEntity? existing = unitOfWork.LineRepository.GetLineByName(model.Name, cancellationToken);

        if (existing is not null) throw new AlreadyExistsException(Phrases.LineAlreadyExists);

        LineEntity line = LineEntity.Create(model);

        await unitOfWork.LineRepository.AddAsync(line, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return line.Id;
    }

    public async Task<int> UpdateLineNameAsync(int id, string name, CancellationToken cancellationToken)
    {
        LineEntity line = await unitOfWork.LineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.LineNotFound);

        line.SetName(name);

        await unitOfWork.CompleteAsync(cancellationToken);

        return line.Id;
    }

    public async Task<int> UpdateLinePointsAsync(int id, List<Point> points, CancellationToken cancellationToken)
    {
        LineEntity line = await unitOfWork.LineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.LineNotFound);

        line.SetPoints(points);

        await unitOfWork.CompleteAsync(cancellationToken);

        return line.Id;
    }

    public async Task<int> DeleteLineAsync(int id, CancellationToken cancellationToken)
    {
        LineEntity line = await unitOfWork.LineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.LineNotFound);

        unitOfWork.LineRepository.Delete(line, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return id;
    }

    public Task<Page<GetLineModel>> GetLinesAsync(GetLinesFilterModel filter, CancellationToken cancellationToken)
        => unitOfWork.LineRepository.GetLinesAsync(filter, cancellationToken);

    public async Task<List<Point>> GetLinePointsAsync(int id, CancellationToken cancellationToken)
    {
        LineEntity line = await unitOfWork.LineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.LineNotFound);

        return line.Points;
    }
}
