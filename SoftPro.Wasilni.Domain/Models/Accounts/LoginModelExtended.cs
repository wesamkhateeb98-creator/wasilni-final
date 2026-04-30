using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Accounts;

public record LoginModelExtended(int Id, string PhoneNumber, string FirstName, string LastName, DateOnly DateOfBirth, Gender Gender, string Token, DateTime ExpirationDate, Role Role, string RefreshToken, Permission? Permission);
