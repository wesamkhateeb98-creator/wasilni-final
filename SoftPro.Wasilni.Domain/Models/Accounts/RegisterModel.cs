namespace SoftPro.Wasilni.Domain.Models.Accounts;

using SoftPro.Wasilni.Domain.Enums;

public record RegisterModel(string FirstName, string LastName, DateTime DateOfBirth, Gender Gender, string Phonenumber, string Password, Role Role, Guid key);
