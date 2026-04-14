using Domain.Resources;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
using SoftPro.Wasilni.Application.Extensions;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Reports;
using Permission = SoftPro.Wasilni.Domain.Enums.Permission;

namespace SoftPro.Wasilni.Application.Services;

public class BusService(IUnitOfWork unitOfWork, IMemoryCache cache) : IBusService
{
    // ─── Admin CRUD ───────────────────────────────────────────────────────────

    public Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken)
        => unitOfWork.BusRepository.GetAllBusesForAdminAsync(inputModel, cancellationToken);

    public async Task<int> AddAsync(AddBusModel model, CancellationToken cancellationToken)
    {
        BusEntity? existingKey = await unitOfWork.BusRepository.FindByIdempotencyKeyAsync(model.key, cancellationToken);
        if (existingKey is not null)
            throw new AlreadyExistsException(Phrases.PlateAlreadyExists);

        if (await unitOfWork.BusRepository.ExistsPlateAsync(model.Plate, cancellationToken))
            throw new AlreadyExistsException(Phrases.PlateAlreadyExists);

        if (model.LineId.HasValue && !await unitOfWork.LineRepository.AnyAsync(model.LineId.Value, cancellationToken))
            throw new FailedPreconditionException(Phrases.LineNotFound);

        BusEntity bus = BusEntity.Create(model);
        await unitOfWork.BusRepository.AddAsync(bus, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    public async Task<int> UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdWithDriverAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (!string.Equals(bus.Plate, model.Plate, StringComparison.OrdinalIgnoreCase))
            if (await unitOfWork.BusRepository.ExistsPlateExceptAsync(model.Plate, id, cancellationToken))
                throw new AlreadyExistsException(Phrases.PlateAlreadyExists);

        if (model.LineId.HasValue && bus.LineId != model.LineId)
            if (!await unitOfWork.LineRepository.AnyAsync(model.LineId.Value, cancellationToken))
                throw new FailedPreconditionException(Phrases.LineNotFound);

        bus.Update(model);
        await unitOfWork.CompleteAsync(cancellationToken);
        return bus.Id;
    }

    public async Task<int> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        await unitOfWork.Transaction(async () =>
        {
            unitOfWork.BusRepository.Delete(bus, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }, cancellationToken);

        return bus.Id;
    }

    public async Task<int> AddDriverAsync(int busId, int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(busId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is not null)
            throw new FailedPreconditionException(Phrases.AssignedOtherDriver);

        AccountEntity driver = await unitOfWork.AccountRepository.GetByIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.DriverNotFound);

        driver.SetPermission(Permission.Driver);
        bus.AssignDriverId(driverId);

        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    public async Task<int> DeleteDriverAsync(int busId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByIdAsync(busId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is null)
            throw new FailedPreconditionException(Phrases.DriverNotFound);

        AccountEntity driver = await unitOfWork.AccountRepository.GetByIdAsync(bus.DriverId.Value, cancellationToken)
            ?? throw new NotFoundException(Phrases.DriverNotFound);

        driver.ClearPermission();
        bus.UnassignDriver();

        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    // ─── Driver: Bus state ────────────────────────────────────────────────────

    public async Task<DriverBusInfoModel> GetBusInfoAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        return new DriverBusInfoModel(
            bus.Id,
            bus.Plate,
            bus.Color,
            bus.Type,
            bus.Status,
            bus.LineId,
            bus.LineEntity?.Name);

    }

    public async Task<GetActiveBusModel> ActivateBusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        //if (bus.Status == BusStatus.Active)
        //    throw new FailedPreconditionException(Phrases.BusAlreadyActive);

        if (bus.LineId is null)
            throw new FailedPreconditionException(Phrases.LineNotFound);

        bus.Activate();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Set(BusCacheKeys.DriverContext(driverId), new DriverContextCache(bus.Id, bus.LineId!.Value));

        return bus.ToModel(null);
    }

    public async Task<GetActiveBusModel> DeactivateBusAsync(int driverId, CancellationToken cancellationToken)
    {
        BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        //if (bus.Status != BusStatus.Active)
        //    throw new FailedPreconditionException(Phrases.BusNotOnRoad);

        bus.Deactivate();
        await unitOfWork.CompleteAsync(cancellationToken);

        cache.Remove(BusCacheKeys.DriverContext(driverId));

        return bus.ToModel(null);
    }

    public async Task<UpdateLocationResult> UpdateLocationAsync(UpdateBusLocationModel model, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverContext(model.DriverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(model.DriverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(model.DriverId), ctx);
        }
        return new UpdateLocationResult(ctx.BusId, ctx.LineId);
    }

    public async Task<AdjustAnonymousResult> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken)
    {
        if (!cache.TryGetValue(BusCacheKeys.DriverContext(driverId), out DriverContextCache? ctx) || ctx is null)
        {
            BusEntity bus = await unitOfWork.BusRepository.GetByDriverIdAsync(driverId, cancellationToken)
                ?? throw new NotFoundException(Phrases.BusNotFound);

            if (bus.Status != BusStatus.Active)
                throw new FailedPreconditionException(Phrases.BusNotOnRoad);

            ctx = new DriverContextCache(bus.Id, bus.LineId!.Value);
            cache.Set(BusCacheKeys.DriverContext(driverId), ctx);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get or create
        var ridership = await unitOfWork.DailyRidershipRepository
            .GetOrCreateAsync(new IncrementRidershipModel(ctx.LineId, ctx.BusId, today), cancellationToken);

        if (ridership is null)
        {

            ridership = DailyRidershipEntity.Create(ctx.LineId, ctx.BusId, today);

        }

        ridership.AdjustRiders(delta);

        await unitOfWork.DailyRidershipRepository.AddAsync(ridership, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new AdjustAnonymousResult(ctx.BusId, ctx.LineId, ridership.NumberOfRiders);
    }

}
