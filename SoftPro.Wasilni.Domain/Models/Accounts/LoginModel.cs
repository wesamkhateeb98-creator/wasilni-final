namespace SoftPro.Wasilni.Domain.Models.Accounts;

public record LoginModel(string Phonenumber, string Password);

public record RefreshModel(string RefreshToken);