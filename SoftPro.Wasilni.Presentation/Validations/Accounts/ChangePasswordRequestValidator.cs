using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.OldPassword)
            .Must(x => Regex.IsMatch(x, PresentationConsts.passwordExpression))
            .WithName(Title.OldPassword)
            .WithMessage(Phrases.InvalidPassword);

        RuleFor(x => x.NewPassword)
            .Must(x => Regex.IsMatch(x, PresentationConsts.passwordExpression))
            .WithName(Title.Password)
            .WithMessage(Phrases.InvalidPassword);
    }
}
