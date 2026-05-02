using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record GetUsersRequest(
    string? PhoneNumber,
    string? FirstName,
    string? LastName,
    Gender? Gender,
    DateOnly? DateOfBirthFrom,
    DateOnly? DateOfBirthTo,
    int PageNumber,
    int PageSize
);