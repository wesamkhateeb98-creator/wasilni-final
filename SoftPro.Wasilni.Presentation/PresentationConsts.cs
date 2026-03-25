namespace SoftPro.Wasilni.Presentation;

public class PresentationConsts
{
    public const string phonenumberExpression = @"^09[0-9]{8}$";
    public const string passwordExpression = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\w\s]).{8,20}$";
    public const string plateExpression = @"^[0-9]{6}$";
}
