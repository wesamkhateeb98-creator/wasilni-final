using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Username)
            .Length(1, 20)
            .WithName(Title.Username)
            .WithMessage(Phrases.InvalidUsername);
    }
}
