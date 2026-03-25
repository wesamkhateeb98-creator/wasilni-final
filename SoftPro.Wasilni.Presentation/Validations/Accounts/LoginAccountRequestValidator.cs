using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class LoginAccountRequestValidator : AbstractValidator<LoginAccountRequest>
{
    public LoginAccountRequestValidator()
    {
        RuleFor(x => x.Phonenumber)
            .Must(x => Regex.IsMatch(x, PresentationConsts.phonenumberExpression))
            .WithName(Title.Phonenumber)
            .WithMessage(Phrases.InvalidPhonenumber);

        RuleFor(x => x.Password)
            .Must(x => Regex.IsMatch(x, PresentationConsts.passwordExpression))
            .WithName(Title.Password)
            .WithMessage(Phrases.InvalidPassword);
    }
}