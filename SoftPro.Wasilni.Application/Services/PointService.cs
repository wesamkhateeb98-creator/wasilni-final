using Domain.Resources;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;

namespace SoftPro.Wasilni.Application.Services;

public class PointService(IUnitOfWork unitOfWork) : IPointService
{
    public async Task<int> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        PointEntity? point = await unitOfWork.PointRepository.GetByIdAsync(id, cancellationToken);

        //if (point is null) throw new NotFoundException(Phrases.PointNotFound);

        //if (point.IsLocked()) throw new FailedPreconditionException(Phrases.PointCannotBeUpdated);

        unitOfWork.PointRepository.Delete(point,cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return id;
    }

    public async Task<Page<GetPointsModel>> GetPointsForAdminAsync(GetModelPaged input,CancellationToken cancellationToken)
    {
        Page<GetPointsModel> points = await unitOfWork.PointRepository.GetAllPoints(input,cancellationToken);

        return points;
   
    }

    public async Task<int> RegisterAsync(RegisterPointModel registerModel, CancellationToken cancellationToken)
    {
        PointEntity point = PointEntity.Create(registerModel);

        await unitOfWork.PointRepository.AddAsync(point, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return point.Id;
    }

    public async Task<GetPointsModel> UpdatePointAsync(UpdatePointModel input, CancellationToken cancellationToken)
    {
        PointEntity? point = await unitOfWork.PointRepository.GetByIdAsync(input.Id, cancellationToken);

        //if (point is null) throw new NotFoundException(Phrases.PointNotFound);

        //if (point.IsLocked()) throw new FailedPreconditionException(Phrases.PointCannotBeUpdated);

        point.Update(input.Latitude, input.Longitude);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new GetPointsModel(point.Id, point.Latitude, point.Longitude, point.LineId);
    }

    public Task<List<GetPointsModel>> GetLinePointsAsync(int lineId, CancellationToken cancellationToken)
        => unitOfWork.PointRepository.GetPointsByLineIdAsync(lineId, cancellationToken);

    public async Task<int> AddLinePointAsync(AddLinePointModel model, CancellationToken cancellationToken)
    {
        PointEntity point = PointEntity.Create(new RegisterPointModel(model.Latitude, model.Longitude), model.LineId);

        await unitOfWork.PointRepository.AddAsync(point, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return point.Id;
    }

    public async Task<GetPointsModel> UpdateLinePointAsync(UpdateLinePointModel model, CancellationToken cancellationToken)
    {
        PointEntity? point = await unitOfWork.PointRepository.GetPointForLineAsync(model.LineId, model.PointId, cancellationToken)
            ?? throw new NotFoundException(Phrases.PointNotFound);

        point.Update(model.Latitude, model.Longitude);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new GetPointsModel(point.Id, point.Latitude, point.Longitude, point.LineId);
    }
}
