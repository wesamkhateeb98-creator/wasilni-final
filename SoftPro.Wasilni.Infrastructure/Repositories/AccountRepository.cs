using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class AccountRepository(AppDbContext dbContext) : Repository<AccountEntity>(dbContext), IAccountRepository
{
    public Task<bool> ExistsPhoneNumberAsync(string phonenumber, CancellationToken cancellationToken)
        => dbContext.Accounts.AnyAsync(x => x.PhoneNumber == phonenumber,cancellationToken);


    public async Task<Page<SearchByPhoneNumberModel>> GetByFilter(int pageNumber, int pageSize, string? phonenumber, CancellationToken cancellationToken)
    {
        IQueryable<AccountEntity> accounts = dbContext.Accounts.AsNoTracking();

        if (!string.IsNullOrEmpty(phonenumber))
        {
            accounts = accounts.Where(x => x.PhoneNumber.StartsWith(phonenumber));
        }

        int count = await accounts.CountAsync(cancellationToken);
        List<SearchByPhoneNumberModel> accountInfo =
            await accounts
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SearchByPhoneNumberModel(x.Id, x.FirstName, x.LastName, x.DateOfBirth, x.Gender, x.PhoneNumber))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        return new(
                pageNumber,
                pageSize,
                (int)Math.Ceiling((double)count / pageSize),
                accountInfo
            );
    }

    public Task<AccountEntity?> GetMatchPhonenumber(string phonenumber, CancellationToken cancellationToken)
        => dbContext.Accounts.FirstOrDefaultAsync(x => x.PhoneNumber == phonenumber, cancellationToken);

    public Task<AccountEntity?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        => dbContext.Accounts.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);

    public Task<AccountEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken)
        => dbContext.Accounts.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
}
