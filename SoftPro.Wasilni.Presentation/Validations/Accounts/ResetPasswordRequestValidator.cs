using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Phonenumber)
            .Must(x => Regex.IsMatch(x, PresentationConsts.phonenumberExpression))
            .WithName(Title.Phonenumber)
            .WithMessage(Phrases.InvalidPhonenumber);

        RuleFor(x => x.Code)
            .Length(6)
            .WithName(Title.Code)
            .WithMessage(Phrases.InvalidCode);

        RuleFor(x => x.NewPassword)
            .Must(x => Regex.IsMatch(x, PresentationConsts.passwordExpression))
            .WithName(Title.Password)
            .WithMessage(Phrases.InvalidPassword);
    }
}
