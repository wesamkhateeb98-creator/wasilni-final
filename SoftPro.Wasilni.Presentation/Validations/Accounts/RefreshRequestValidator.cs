using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithName(Title.RefreshToken)
            .WithMessage(Phrases.InvalidRefreshToken);
    }
}