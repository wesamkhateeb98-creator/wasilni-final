using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Accounts;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IAccountService
{
    Task<LoginModelExtended> LoginAsync(LoginModel loginModel, CancellationToken cancellationToken);
    Task<int> SignUpAsync(RegisterModel registerModel, CancellationToken cancellationToken);
    Task<Page<SearchByPhoneNumberModel>> GetByPhonenumberAsync(int pageNumber, int pageSize, string? phonenumber, CancellationToken cancellationToken);
    Task<int> SendCodeAsync(string phonenumber, SendCodePurpose purpose, CancellationToken cancellationToken);
    Task<LoginModelExtended> VerifyAccountAsync(string phonenumber, string code, CancellationToken cancellationToken);
    Task<int> ChangePasswordAsync(int id, string oldPassword, string newPassword, CancellationToken cancellationToken);
    Task<int> ResetPasswordAsync(string phonenumber, string code, string newPassword, CancellationToken cancellationToken);
    Task<int> UpdateProfileAsync(int v, string username, CancellationToken cancellationToken);
    Task<LoginModelExtended> RefreshAsync(RefreshModel refreshModel, CancellationToken cancellationToken);
}
