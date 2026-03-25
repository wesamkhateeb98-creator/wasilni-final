namespace SoftPro.Wasilni.Domain.Models.Accounts;

public record LoginModel(string Phonenumber, string Password, string FCMToken);

public record RefreshModel(string RefreshToken);