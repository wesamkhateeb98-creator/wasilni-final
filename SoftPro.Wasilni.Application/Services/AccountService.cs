using Domain.Resources;
using Microsoft.Extensions.Options;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Extensions;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Exceptions;
using SoftPro.Wasilni.Domain.Helper;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Accounts;
using SoftPro.Wasilni.Domain.Options;
using System.Security.Claims;

namespace SoftPro.Wasilni.Application.Services;

public class AccountService(IUnitOfWork unitOfWork, IWhatsAppRepository WhatsAppRepository, IOptions<JwtOption> jwtOption) : IAccountService
{
    private int RefreshDays => jwtOption.Value.RefreshTokenDurationDays;

    public async Task<Page<SearchByPhoneNumberModel>> GetByPhonenumberAsync(int pageNumber, int pageSize, string? phonenumber, CancellationToken cancellationToken)
       => await unitOfWork.AccountRepository.GetByFilter(pageNumber, pageSize, phonenumber, cancellationToken);


    public async Task<LoginModelExtended> LoginAsync(LoginModel loginModel, CancellationToken cancellationToken)
    {
        AccountEntity? account = await unitOfWork.AccountRepository.GetMatchPhonenumber(loginModel.Phonenumber, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        if (!account.Confirmed)
            throw new FailedPreconditionException(Phrases.AccountNotConfirmed);

        bool matchPassword = account.Password.SequenceEqual(AuthHelper.HashPasswordWithSalt(loginModel.Password, account.Salt));

        if (!matchPassword)
            throw new NotFoundException(Phrases.UsernameOrPasswordIsInvalid);

        List<Claim> claims = account.GetClaim();

        account.ChangeRefreshToken(AuthHelper.GenerateRefreshToken(), RefreshDays);

        await unitOfWork.CompleteAsync(cancellationToken);

        var (token, expirationDate) = AuthHelper.GenerateToken(claims, jwtOption.Value);

        return new(account.Id, account.PhoneNumber, account.Name, token, expirationDate, account.Role, account.RefreshToken, account.Permission);
    }

    public async Task<int> SignUpAsync(RegisterModel registerModel, CancellationToken cancellationToken)
    {
        bool isExists = await unitOfWork.AccountRepository.ExistsPhoneNumberAsync(registerModel.Phonenumber, cancellationToken);

        if (isExists)
            throw new AlreadyExistsException(Phrases.PhonenumberAlreadyExsits);

        byte[] salt = AuthHelper.GenerateSalt();
        byte[] passwordHashed = AuthHelper.HashPasswordWithSalt(registerModel.Password, salt);
        string refreshToken = AuthHelper.GenerateRefreshToken();
        string code = AuthHelper.GenerateCode();

        AccountEntity account = AccountEntity.Create(registerModel, passwordHashed, salt, refreshToken, code, RefreshDays);

        if (account.SendCodeCount <= 0 && account.CodeExpiration.HasValue && DateTime.UtcNow > account.CodeExpiration.Value.AddMinutes(30))
            account.SetCountCode(3);

        account.SetCodeExpiration(DateTime.UtcNow.AddMinutes(10));
        account.MinusCountCode();

        await unitOfWork.AccountRepository.AddAsync(account, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        bool sendSucceeded = await WhatsAppRepository.SendCode(registerModel.Phonenumber, code, cancellationToken);
        if (!sendSucceeded)
            throw new FailedPreconditionException(Phrases.SendCodeFailed);

        return account.Id;
    }

    public async Task<int> SendCodeAsync(string phonenumber, CancellationToken cancellationToken)
    {
        AccountEntity account = await unitOfWork.AccountRepository.GetMatchPhonenumber(phonenumber, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        //if (account.Confirmed)
        //    throw new FailedPreconditionException(Phrases.AccountAlreadyConfirmed);

        if (account.SendCodeCount <= 0 && account.CodeExpiration.HasValue && DateTime.UtcNow > account.CodeExpiration.Value.AddMinutes(30))
            account.SetCountCode(3);

        if (account.SendCodeCount <= 0)
            throw new FailedPreconditionException(Phrases.SendCodeMoreTime);

        string code = AuthHelper.GenerateCode();

        account.SetCode(code);

        account.SetCodeExpiration(DateTime.UtcNow.AddMinutes(10));

        account.MinusCountCode();

        await unitOfWork.CompleteAsync(cancellationToken);

        bool sendSucceeded = await WhatsAppRepository.SendCode(account.PhoneNumber, code, cancellationToken);

        if (!sendSucceeded)
            throw new FailedPreconditionException(Phrases.SendCodeFailed);

        return account.Id;
    }

    public async Task<LoginModelExtended> VerifyAccountAsync(string phonenumber, string code, CancellationToken cancellationToken)
    {
        AccountEntity account = await unitOfWork.AccountRepository.GetMatchPhonenumber(phonenumber, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        if (account.Code is null)
            throw new FailedPreconditionException(Phrases.YouHaveNotSentCode);

        if (account.Code != code)
            throw new FailedPreconditionException(Phrases.CannotMatchCode);

        account.ConfirmAccount();
        account.ChangeRefreshToken(AuthHelper.GenerateRefreshToken(), RefreshDays);

        await unitOfWork.CompleteAsync(cancellationToken);

        List<Claim> claims = account.GetClaim();
        var (token, expirationDate) = AuthHelper.GenerateToken(claims, jwtOption.Value);

        return new(account.Id, account.PhoneNumber, account.Name, token, expirationDate, account.Role, account.RefreshToken, account.Permission);
    }

    public async Task<int> ChangePasswordAsync(int id, string oldPassword, string newPassword, CancellationToken cancellationToken)
    {
        AccountEntity account = await unitOfWork.AccountRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        byte[] oldeHashPassword = AuthHelper.HashPasswordWithSalt(oldPassword, account.Salt),
                newHashPassword = AuthHelper.HashPasswordWithSalt(newPassword, account.Salt);

        if (!oldeHashPassword.SequenceEqual(account.Password))
            throw new FailedPreconditionException(Phrases.MatchOld);

        if (oldeHashPassword.SequenceEqual(newHashPassword))
            throw new FailedPreconditionException(Phrases.NewPasswordNoChanged);

        account.SetPassword(newHashPassword);
        await unitOfWork.CompleteAsync(cancellationToken);

        return account.Id;
    }

    public async Task<int> ResetPasswordAsync(string phonenumber, string code, string newPassword, CancellationToken cancellationToken)
    {
        AccountEntity account = await unitOfWork.AccountRepository.GetMatchPhonenumber(phonenumber, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        if (account.Code is null)
            throw new FailedPreconditionException(Phrases.YouHaveNotSentCode);

        if (account.Code != code || !account.CodeExpiration.HasValue || DateTime.UtcNow > account.CodeExpiration.Value)
            throw new FailedPreconditionException(Phrases.CannotMatchCode);

        byte[] newHashPassword = AuthHelper.HashPasswordWithSalt(newPassword, account.Salt);

        if (newHashPassword.SequenceEqual(account.Password))
            throw new FailedPreconditionException(Phrases.NewPasswordNoChanged);

        account.SetPassword(newHashPassword);
        account.SetCode(null);
        account.SetCodeExpiration(null);

        await unitOfWork.CompleteAsync(cancellationToken);

        return account.Id;
    }

    public async Task<int> UpdateProfileAsync(int id, string username, CancellationToken cancellationToken)
    {
        AccountEntity account = await unitOfWork.AccountRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(Phrases.NotFound);

        if (account.ChangeData(username))
            throw new FailedPreconditionException(Phrases.AccountDataNoChanged);

        account.SetName(username);
        await unitOfWork.CompleteAsync(cancellationToken);

        return account.Id;
    }

    public async Task<LoginModelExtended> RefreshAsync(RefreshModel refreshModel, CancellationToken cancellationToken)
    {
        AccountEntity? account = await unitOfWork.AccountRepository.GetByRefreshTokenAsync(refreshModel.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException(Phrases.InvalidIdOrRefreshToken);

        if (account.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new ForbiddenException(Phrases.RefreshTokenExpired);

        if (!account.Confirmed)
            throw new FailedPreconditionException(Phrases.AccountNotConfirmed);

        List<Claim> claims = account.GetClaim();
        var (token, expirationDate) = AuthHelper.GenerateToken(claims, jwtOption.Value);

        account.ChangeRefreshToken(AuthHelper.GenerateRefreshToken(), RefreshDays);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new(account.Id, account.PhoneNumber, account.Name, token, expirationDate, account.Role, account.RefreshToken, account.Permission);
    }
}
