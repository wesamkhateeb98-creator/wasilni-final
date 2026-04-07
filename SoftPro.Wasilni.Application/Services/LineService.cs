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
        // Idempotency check
        LineEntity? line = await unitOfWork.LineRepository.FindByIdempotencyKeyAsync(model.key, cancellationToken);
        if (line is not null)
            throw new AlreadyExistsException(Phrases.LineAlreadyExists);

        LineEntity? existing = unitOfWork.LineRepository.GetLineByName(model.Name, cancellationToken);
        if (existing is not null) throw new AlreadyExistsException(Phrases.LineAlreadyExists);

        LineEntity newLine = LineEntity.Create(model);
        await unitOfWork.LineRepository.AddAsync(newLine, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return newLine.Id;
    }

    public async Task<int> UpdateLineAsync(int id, UpdateLineModel model, CancellationToken cancellationToken)
    {
        LineEntity line = await unitOfWork.LineRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.LineNotFound);

        line.SetName(model.Name);
        line.SetPoints(model.Points);

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
