using Domain.Resources;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;

namespace SoftPro.Wasilni.Application.Services;

public class BusService(IUnitOfWork unitOfWork) : IBusService
{
    public Task<Page<GetBusesModel>> GetBusesAsync(GetBusModel inputModel, CancellationToken cancellationToken)
        => unitOfWork.BusRepository.GetAllBusesForPassengerIdAsync(inputModel, cancellationToken);

    public Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken)
        => unitOfWork.BusRepository.GetAllBusesForAdminAsync(inputModel, cancellationToken);

    public async Task<int> RegisterAsync(RegisterBusModel registerModel, CancellationToken cancellationToken)
    {
        AccountEntity? account = await unitOfWork.AccountRepository.GetByIdAsync(registerModel.accountId, cancellationToken)
            ?? throw new NotFoundException(Phrases.AccountNotRegistered);
        bool isExists = await unitOfWork.BusRepository.ExistsPlateAsync(registerModel.Plate, cancellationToken);

        if (isExists)
        {
            throw new AlreadyExistsException(Phrases.PlateAlreadyExists);
        }

        bool existsLine = await unitOfWork.LineRepository.AnyAsync(registerModel.lineId, cancellationToken);

        if (!existsLine)
            throw new FailedPreconditionException(Phrases.LineNotFound);

        BusEntity bus = BusEntity.Create(registerModel);

        await unitOfWork.BusRepository.AddAsync(bus, cancellationToken);

        account.ChangePermission(Permission.ControlBus);

        await unitOfWork.CompleteAsync(cancellationToken);
        return bus.Id;
    }

    public async Task<int> UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken)
    {
        BusEntity? bus = await unitOfWork.BusRepository.GetByIdWithDriverAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);

        if (!string.Equals(bus.Plate, model.Plate, StringComparison.OrdinalIgnoreCase))
        {
            bool plateExists = await unitOfWork.BusRepository.ExistsPlateExceptAsync(model.Plate, id, cancellationToken);
            if (plateExists)
                throw new AlreadyExistsException(Phrases.PlateAlreadyExists);
        }

        if (bus.LineId != model.LineId)
        {
            bool lineExists = await unitOfWork.LineRepository.AnyAsync(model.LineId, cancellationToken);
            if (!lineExists)
                throw new FailedPreconditionException(Phrases.LineNotFound);
        }

        bus.Update(model);

        await unitOfWork.CompleteAsync(cancellationToken);

        return bus.Id;
    }

    public async Task<int> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        BusEntity? busEntity = await unitOfWork.BusRepository.GetWithRequestByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.BusNotFound);
       
        await unitOfWork.Transaction(async () =>
        {
            unitOfWork.BusRepository.Delete(busEntity, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);

        }, cancellationToken);

        return busEntity.Id;
    }

    public async Task<GetBusesForAdminModel> AddDriver(AddDriverOnBusModel model, CancellationToken cancellationToken)
    {
        BusEntity? bus = await unitOfWork.BusRepository.GetByIdAsync(model.BusId, cancellationToken);

        if (bus is null) throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is not null) throw new FailedPreconditionException(Phrases.AssignedOtherDriver);
        
        AccountEntity? account = await unitOfWork.AccountRepository.GetByIdAsync(model.DriverId, cancellationToken);

        if (account is null) throw new NotFoundException(Phrases.DriverNotFound);

        if (account.Permission == Permission.BusDriving) throw new FailedPreconditionException(Phrases.DriverAlreadyExists);

        account.ChangePermission(Permission.BusDriving);

        bus.AssignDriverId(model.DriverId);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new(bus.Id,bus.Plate,bus.Color,bus.Type,bus.NumberOfSeats,bus.LineId,new(model.DriverId,account.Name));
    }

    public async Task<GetBusesForAdminModel> DeleteDriver(DeleteDriverFromBusModel model, CancellationToken cancellationToken)
    {
        BusEntity? bus = await unitOfWork.BusRepository.GetByIdAsync(model.BusId, cancellationToken);

        if (bus is null) throw new NotFoundException(Phrases.BusNotFound);

        if (bus.DriverId is null) throw new FailedPreconditionException(Phrases.DriverNotFound);

        AccountEntity? account = await unitOfWork.AccountRepository.GetByIdAsync(model.DriverId, cancellationToken);

        if (account is null) throw new NotFoundException(Phrases.DriverNotFound);

        if (account.Permission != Permission.BusDriving) throw new FailedPreconditionException(Phrases.DriverNotFound);

        account.ChangePermission(Permission.None);

        bus.UnassignDriver();

        await unitOfWork.CompleteAsync(cancellationToken);

        return new(bus.Id, bus.Plate, bus.Color, bus.Type, bus.NumberOfSeats, bus.LineId, new(model.DriverId, account.Name));
    }

}
