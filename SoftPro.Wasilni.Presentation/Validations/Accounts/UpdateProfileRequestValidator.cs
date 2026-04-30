using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .Length(1, 20)
            .WithName(Title.FirstName)
            .WithMessage(Phrases.InvalidFirstName);

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .Length(1, 20)
            .WithName(Title.LastName)
            .WithMessage(Phrases.InvalidLastName);

        RuleFor(x => x.DateOfBirth)
            .Cascade(CascadeMode.Stop)
            .LessThan(DateTime.Today.AddYears(-7))
            .WithName(Title.DateOfBirth)
            .WithMessage(Phrases.InvalidDateOfBirth);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithName(Title.Gender)
            .WithMessage(Phrases.InvalidGender);

    }
}
