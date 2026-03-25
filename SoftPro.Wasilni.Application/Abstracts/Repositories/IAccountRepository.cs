using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Accounts;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IAccountRepository : IRepository<AccountEntity>
{
    Task<bool> ExistsPhoneNumberAsync(string phonenumber, CancellationToken cancellationToken);
    Task<Page<SearchByPhoneNumberModel>> GetByFilter(int pageNumber, int pageSize, string? phonenumber, CancellationToken cancellationToken);
    Task<AccountEntity?> GetMatchPhonenumber(string phonenumber, CancellationToken cancellationToken);
    Task<AccountEntity?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
