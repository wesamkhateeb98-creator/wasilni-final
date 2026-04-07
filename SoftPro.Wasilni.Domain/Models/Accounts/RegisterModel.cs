namespace SoftPro.Wasilni.Domain.Models.Accounts;

using SoftPro.Wasilni.Domain.Enums;

public record RegisterModel(string Username, string Phonenumber, string Password, string FCMToken, Role Role, Guid key);
