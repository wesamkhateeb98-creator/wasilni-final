using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Accounts;

public record RegisterModel(string Username, string Phonenumber, string Password,string FCMToken, Role Role);
