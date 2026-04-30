using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Account;

public record UserResponse(int Id, string FirstName, string LastName, DateOnly DateOfBirth, Gender Gender, string PhoneNumber);
