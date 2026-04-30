using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Account;

public record TokenResponse(int Id, string PhoneNumber, string FirstName, string LastName, DateOnly DateOfBirth, Gender Gender, string Token, DateTime ExpirationDate, Role Role, string RefreshToken, Permission? Permission);
