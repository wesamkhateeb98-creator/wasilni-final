
using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
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

    public async Task<BusEntity?> GetByIdWithDriverAsync(int busId, CancellationToken cancellationToken)
    {
        return await dbContext.Buses
            .Include(b => b.Driver)
            .FirstOrDefaultAsync(b => b.Id == busId, cancellationToken);
    }

    public async Task<Page<GetBusesForAdminModel>> GetAllBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken)
    {
        IQueryable<BusEntity> query = dbContext.Buses
            .Include(x => x.LineEntity)
            .AsQueryable();

        if (inputModel.Plate is not null)
            query = query.Where(x => x.Plate.Contains(inputModel.Plate));

        if (inputModel.Type.HasValue)
            query = query.Where(x => x.Type == inputModel.Type.Value);

        int count = await query.CountAsync(cancellationToken);

        List<GetBusesForAdminModel> result = await query
             .OrderByDescending(x => x.Id)
             .Select(x => new GetBusesForAdminModel(
                 x.Id,
                 x.Plate,
                 x.Color,
                 x.Type,
                 x.NumberOfSeats,
                 x.LineId,
                 x.LineId.HasValue ? x.LineEntity!.Name : null,
                 x.DriverId.HasValue
                    ? new(
                        x.DriverId.Value,
                        (x.Driver!.FirstName + " " + x.Driver.LastName).Trim())
                    : null
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

    public Task<BusEntity?> GetByDriverIdAsync(int driverId, CancellationToken cancellationToken)
        => dbContext.Buses
            .Include(b => b.LineEntity)
            .FirstOrDefaultAsync(b => b.DriverId == driverId, cancellationToken);

    public Task<BusEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken)
        => dbContext.Buses.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

    public Task<bool> HasActiveBusOnLineAsync(int lineId, CancellationToken cancellationToken)
        => dbContext.Buses.AnyAsync(
            x => x.LineId == lineId && x.Status == BusStatus.Active,
            cancellationToken);
}
