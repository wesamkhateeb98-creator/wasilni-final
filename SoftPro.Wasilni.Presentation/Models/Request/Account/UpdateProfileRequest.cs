using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record UpdateProfileRequest(string FirstName, string LastName, DateTime DateOfBirth, Gender Gender);
