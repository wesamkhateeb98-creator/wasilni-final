using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Account;

public record SearchByPhonenumberResponse(int Id, string FirstName, string LastName, DateOnly DateOfBirth, Gender Gender, string PhoneNumber);
