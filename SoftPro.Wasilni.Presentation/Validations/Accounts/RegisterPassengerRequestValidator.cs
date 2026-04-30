using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class RegisterPassengerRequestValidator : AbstractValidator<SignupPassengerRequest>
{
    public RegisterPassengerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(1, 20)
            .WithName(Title.FirstName)
            .WithMessage(Phrases.InvalidFirstName);

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(1, 20)
            .WithName(Title.LastName)
            .WithMessage(Phrases.InvalidLastName);

        RuleFor(x => x.DateOfBirth)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .LessThan(DateTime.Today.AddYears(-7))
            .WithName(Title.DateOfBirth)
            .WithMessage(Phrases.InvalidDateOfBirth);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithName(Title.Gender)
            .WithMessage(Phrases.InvalidGender);

        //RuleFor(x => x.Phonenumber)
        //    .Cascade(CascadeMode.Stop)
        //    .NotEmpty()
        //    .Must(x => !string.IsNullOrWhiteSpace(x) && Regex.IsMatch(x, PresentationConsts.phonenumberExpression))
        //    .WithName(Title.Phonenumber)
        //    .WithMessage(Phrases.InvalidPhonenumber);

        RuleFor(x => x.Password)
            .Must(x => Regex.IsMatch(x, PresentationConsts.passwordExpression))
            .WithName(Title.Password)
            .WithMessage(Phrases.InvalidPassword);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(password => !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, PresentationConsts.passwordExpression))
            .WithName(Title.Password)
            .WithMessage(Phrases.InvalidPassword);

        //RuleFor(x => x.key)
        //    .NotEqual(Guid.Empty)
        //    .WithMessage(Phrases.InvalidKey);

    }
}
