namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record GetUsersRequest(string? PhoneNumber, int PageNumber, int PageSize);
