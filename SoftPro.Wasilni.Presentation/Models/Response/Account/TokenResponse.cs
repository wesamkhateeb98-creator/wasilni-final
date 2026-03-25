using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Account;

public record TokenResponse(int Id,string PhoneNumber,string Name, string Token, DateTime ExpirationDate, Role Role,Permission Permission, string RefreshToken,string FCMToken);
