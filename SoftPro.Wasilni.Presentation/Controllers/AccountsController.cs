using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SoftPro.Wasilni.Presentation.Extensions;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.AccountExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Account;


namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
public class AccountsController(IAccountService accountService) : BaseController
{
    [HttpPost("sign-up")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IdResponse> RegisterAsync([FromBody] SignupPassengerRequest registerRequest, CancellationToken cancellationToken)
    {
        int id = await accountService.SignUpAsync(registerRequest.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<TokenResponse> LoginAsync([FromBody] LoginAccountRequest loginPassengerRequest, CancellationToken cancellationToken)
    {
        LoginModelExtended loginModelExtended = await accountService.LoginAsync(loginPassengerRequest.ToModel(), cancellationToken);
        return loginModelExtended.ToResponse();
    }

    [HttpPost("refresh")]
    public async Task<TokenResponse> RefreshAsync([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        LoginModelExtended loginModelExtended = await accountService.RefreshAsync(request.ToModel(), cancellationToken);
        return loginModelExtended.ToResponse();
    }

    [HttpGet]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<Page<UserResponse>> GetUsersAsync([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        Page<SearchByPhoneNumberModel> data = await accountService.GetByPhonenumberAsync(request.PageNumber, request.PageSize, request.PhoneNumber, cancellationToken);
        return data.ToUserResponse();
    }

    [HttpGet("search/by-phonenumber")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<Page<SearchByPhonenumberResponse>> SearchByPhonenumberAsync([FromQuery] SearchAccountByPhoneNumberRequest request, CancellationToken cancellationToken)
    {
        Page<SearchByPhoneNumberModel> data = await accountService.GetByPhonenumberAsync(request.PageNumber, request.PageSize, request.Phonenumber, cancellationToken);
        return data.ToResponse();
    }

    [HttpPost("send-code")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IdResponse> SendCodeAsync([FromBody]SendCodeRequest request , CancellationToken cancellationToken)
    {
        int id = await accountService.SendCodeAsync(request.Phonenumber, cancellationToken);
        return new(id);
    }

    [HttpPost("verify-account")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<TokenResponse> VerifyAccountAsync([FromBody] ConfirmCodeRequest request, CancellationToken cancellationToken)
    {
        LoginModelExtended loginModelExtended = await accountService.VerifyAccountAsync(request.Phonenumber, request.Code , cancellationToken);
        return loginModelExtended.ToResponse();
    }

    [HttpPatch("change-password")]
    [Authorize]
    public async Task<IdResponse> ChangePasswordAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        int id = await accountService.ChangePasswordAsync(User.GetId(), request.OldPassword,request.NewPassword, cancellationToken);
        return new(id);
    }

    [HttpPatch("update-profile")]
    [Authorize]
    public async Task<IdResponse> UpdateProfileAsync([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        int id = await accountService.UpdateProfileAsync(User.GetId(), request.Username, cancellationToken);
        return new(id);
    }
}

//Todo: Polly
//Rate limit
// in memory