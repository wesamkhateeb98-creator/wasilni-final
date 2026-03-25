namespace SoftPro.Wasilni.Presentation.Models.Request.Account;

public record SignupPassengerRequest(string Username, string Phonenumber, string Password,string FCMToken);