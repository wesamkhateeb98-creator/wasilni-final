
using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Infrastructure.Persistence;


namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class BusRepository(AppDbContext dbContext) : Repository<BusEntity>(dbContext), IBusRepository
{
    public Task<bool> ExistsPlateAsync(string plate, CancellationToken cancellationToken)
        => dbContext.Buses.AnyAsync(x => x.Plate == plate, cancellationToken);

    public Task<bool> ExistsPlateExceptAsync(string plate, int excludeId, CancellationToken cancellationToken)
        => dbContext.Buses.AnyAsync(x => x.Plate == plate && x.Id != excludeId, cancellationToken);

    public async Task<Page<GetBusesModel>> GetAllBusesForPassengerIdAsync(GetBusModel inputFilter, CancellationToken cancellationToken)
    {
        IQueryable<BusEntity> query = dbContext.Buses
            .Include(x => x.LineEntity)
            .Include(x => x.Driver)
            .AsQueryable();

        List<GetBusesModel> result = await query
             .Select(x => new GetBusesModel(
                    x.Id,
                    x.OwnId.HasValue ? new UsernameModel(x.OwnId.Value, x.Own!.Name) : null,
                    x.Plate,
                    x.Color,
                    x.Type,
                    x.LineId.HasValue ? new LineBusModel(x.LineId.Value, x.LineEntity!.Name) : null,
                    x.DriverId.HasValue ? new UsernameModel(x.DriverId.Value, x.Driver!.Name) : null
                 ))
             .Skip((inputFilter.pageNumber - 1) * inputFilter.PageSize)
             .Take(inputFilter.PageSize)
             .ToListAsync(cancellationToken);

        int count = await query.CountAsync(cancellationToken);

        return new(
             inputFilter.pageNumber,
             inputFilter.PageSize,
             (int)Math.Ceiling((double)count / inputFilter.PageSize),
             result
            );
    }

    public Task<BusEntity?> GetWithRequestByBusIdAsync(int busId, CancellationToken cancellationToken)
        //=> dbContext.Buses
        //    .Include(x => x.Requests)
        //    .FirstOrDefaultAsync(x => x.Id == busId, cancellationToken);
        => null;

    public async Task<BusEntity?> GetByIdWithDriverAsync(int busId, CancellationToken cancellationToken)
    {
        return await dbContext.Buses
            .Include(b => b.Driver)
            .FirstOrDefaultAsync(b => b.Id == busId, cancellationToken);
    }

    public Task<BusEntity?> GetWithBusWithRequestBusByIdAsync(int busId, CancellationToken cancellationToken)
        //=> dbContext.Buses.Include(x => x.Requests).Include(x => x.Own).FirstOrDefaultAsync(x => x.Id == busId,cancellationToken);
        => null;
    public Task<BusEntity?> GetWithRequestByIdAsync(int id, CancellationToken cancellationToken)
        //=> dbContext.Buses.Include(x => x.Requests).FirstOrDefaultAsync(x => x. == id, cancellationToken);
        => null;

    public async Task<Page<GetBusesForAdminModel>> GetAllBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken)
    {
        IQueryable<BusEntity> query = dbContext.Buses
            .Include(x => x.LineEntity)
            .AsQueryable();
        //if (inputModel.Filter != BusTypeFilter.All)
        //{
        //    BusType type = inputModel.Filter switch
        //    {
        //        BusTypeFilter.Bolman => BusType.Bolman,
        //        BusTypeFilter.Van => BusType.Van,
        //        _ => BusType.Servece
        //    };
        //    query = query.Where(x => x.Type == type);
        //}

        if (inputModel.Plate is not null)
        {
            query = query.Where(x => x.Plate == inputModel.Plate);
        }

        //if (inputModel.OwnerId is not null)
        //{
        //    query = query.Where(x => x.OwnId == inputModel.OwnerId);
        //}

        int count = await query.CountAsync(cancellationToken);

        List<GetBusesForAdminModel> result = await query
             .Select(x => new GetBusesForAdminModel(
                 x.Id,
                 x.Plate,
                 x.Color,
                 x.Type,
                 x.NumberOfSeats,
                 x.LineId,
                 x.DriverId.HasValue ? new(x.DriverId.Value, x.Driver.Name) : null
             ))
             .Skip((inputModel.pageNumber - 1) * inputModel.PageSize)
             .Take(inputModel.PageSize)
             .ToListAsync(cancellationToken);

        return new(
             inputModel.pageNumber,
             inputModel.PageSize,
             (int)Math.Ceiling((double)count / inputModel.PageSize),
             result
             );

    }

    public Task<BusEntity?> GetByIdWithLineAsync(int id, CancellationToken cancellationToken)
        => dbContext.Buses
            .Include(b => b.LineEntity)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<BusEntity?> GetByDriverIdAsync(int driverId, CancellationToken cancellationToken)
        => dbContext.Buses
            .Include(b => b.LineEntity)
            .FirstOrDefaultAsync(b => b.DriverId == driverId, cancellationToken);

    public Task<bool> AnyByDriverAsync(int busId, int driverId, CancellationToken cancellationToken)
        => dbContext.Buses.AnyAsync(x => x.Id == busId && x.DriverId == driverId, cancellationToken);

    public Task<bool> HasBusAsync(int driverId, CancellationToken cancellationToken)
        => dbContext.Buses.AnyAsync(x => x.DriverId == driverId, cancellationToken);

    public Task<BusEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken)
        => dbContext.Buses.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
}
