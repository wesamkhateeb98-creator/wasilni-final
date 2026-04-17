namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record ResetPasswordRequest(string Phonenumber, string Code, string NewPassword);