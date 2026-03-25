using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class SendCodeRequestValidation:AbstractValidator<SendCodeRequest>
{
    public SendCodeRequestValidation()
    {
        RuleFor(x => x.Phonenumber)
            .Must(x => Regex.IsMatch(x, PresentationConsts.phonenumberExpression))
            .WithName(Title.Phonenumber)
            .WithMessage(Phrases.InvalidPhonenumber);
    }
}
