using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class ConfirmCodeRequestValidation : AbstractValidator<ConfirmCodeRequest>
{
    public ConfirmCodeRequestValidation()
    {
        RuleFor(x => x.Phonenumber)
            .Must(x => Regex.IsMatch(x, PresentationConsts.phonenumberExpression))
            .WithName(Title.Phonenumber)
            .WithMessage(Phrases.InvalidPhonenumber);

        RuleFor(x => x.Code)
            .Length(6)
            .WithName(Title.Code)
            .WithMessage(Phrases.InvalidCode);
    }
}