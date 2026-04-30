using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(1, 20)
            .WithName(Title.Username)
            .WithMessage(Phrases.InvalidUsername);

        RuleFor(x => x.LastName)
            .Length(1, 20)
            .WithName(Title.Username)
            .WithMessage(Phrases.InvalidUsername);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today)
            .WithName(Title.Username)
            .WithMessage(Phrases.InvalidUsername);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithName(Title.Username)
            .WithMessage(Phrases.InvalidUsername);
    }
}
