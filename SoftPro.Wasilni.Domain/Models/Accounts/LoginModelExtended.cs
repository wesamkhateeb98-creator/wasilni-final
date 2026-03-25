using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Accounts;

public record LoginModelExtended(int Id, string PhoneNumber, string Name, string Token, DateTime ExpirationDate, Role Role, Permission Permission, string RefreshToken, string FCMToken);
