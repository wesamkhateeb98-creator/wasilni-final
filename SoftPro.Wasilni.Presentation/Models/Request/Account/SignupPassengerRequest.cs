using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record SignupPassengerRequest(string FirstName, string LastName, DateTime DateOfBirth, Gender Gender, string Phonenumber, string Password, Guid key);
