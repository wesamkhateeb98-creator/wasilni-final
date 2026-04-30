using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Accounts;

public record SearchByPhoneNumberModel(int Id, string FirstName, string LastName, DateOnly DateOfBirth, Gender Gender, string Phonenumber);
