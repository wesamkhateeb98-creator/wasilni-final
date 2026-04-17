using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Accounts;

namespace SoftPro.Wasilni.Domain.Entities;

public class AccountEntity : IEntity
{
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public byte[] Password { get; private set; }
    public byte[] Salt { get; private set; }
    public string? Code { get; private set; }
    public int SendCodeCount { get; private set; }
    public bool Confirmed { get; private set; }
    public DateTime? CodeExpiration { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Role Role { get; private set; }
    public Permission? Permission { get; private set; }
    public string RefreshToken { get; private set; }
    public DateTime RefreshTokenExpiresAt { get; private set; }
    public Guid Key { get; private set; }

    private AccountEntity() { }

    public AccountEntity(
        string name,
        string phoneNumber,
        byte[] password,
        byte[] salt,
        string? code,
        DateTime? codeExpiration,
        int sendCodeCount,
        DateTime createdAt,
        Role role,
        bool confirmed,
        string refreshToken,
        DateTime refreshTokenExpiresAt,
        Guid idempotencyKey
        )
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Password = password;
        Salt = salt;
        Code = code;
        SendCodeCount = sendCodeCount;
        CodeExpiration = codeExpiration;
        CreatedAt = createdAt;
        Role = role;
        Confirmed = confirmed;
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        Key = idempotencyKey;
        Permission = Enums.Permission.Passenger;
    }

    public static AccountEntity Create(RegisterModel registerModel, byte[] passwordHashed, byte[] salt, string refreshToken, string code, int refreshTokenDurationDays)
        => new(
            registerModel.Username,
            registerModel.Phonenumber,
            passwordHashed,
            salt,
            code,
            null,
            3,
            DateTime.UtcNow,
            registerModel.Role,
            false,
            refreshToken,
            DateTime.UtcNow.AddDays(refreshTokenDurationDays),
            registerModel.key
            );

    public void SetCountCode(int codeCount)
        => SendCodeCount = codeCount;

    public void MinusCountCode() => SendCodeCount--;

    public void SetCode(string code)
        => Code = code;

    public void SetCodeExpiration(DateTime? dateTime)
        => CodeExpiration = dateTime;

    public void ConfirmAccount()
    {
        Code = null;
        CodeExpiration = null;
        SendCodeCount = 3;
        Confirmed = true;
    }

    public bool ChangeData(string username)
        => string.Equals(Name, username, StringComparison.OrdinalIgnoreCase);

    public void SetPassword(byte[] newHashPassword)
        => Password = newHashPassword;

    public void SetName(string name)
        => Name = name;

    public void ChangeRefreshToken(string refreshToken, int durationDays)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(durationDays);
    }

    public void SetPermission(Permission permission)
        => Permission = permission;

    public void ClearPermission()
        => Permission = null;
}
