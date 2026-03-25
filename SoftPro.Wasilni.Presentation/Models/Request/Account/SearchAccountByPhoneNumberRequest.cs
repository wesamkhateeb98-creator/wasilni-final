namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record SearchAccountByPhoneNumberRequest(int PageNumber, int PageSize, string? Phonenumber);

public record SearchAccountByPhoneNumberForOwnerRequest(string Phonenumber);
