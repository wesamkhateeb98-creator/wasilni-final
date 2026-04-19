using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record SendCodeRequest(string Phonenumber, SendCodePurpose Purpose);
