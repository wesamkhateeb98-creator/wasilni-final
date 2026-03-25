using System.Text.RegularExpressions;
using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;


namespace SoftPro.Wasilni.Presentation.Validations.Buses;

public class GetBusesForAdminRequestValidator : AbstractValidator<GetBusesForAdminRequest>
{
    public GetBusesForAdminRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithName(Title.PageNumber)
            .WithMessage(Phrases.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithName(Title.PageSize)
            .WithMessage(Phrases.InvalidPageSize);

        RuleFor(x => x.OwnerId)
            .GreaterThan(0)
            .WithName(Title.Id)
            .WithMessage(Phrases.InvalidId);

        RuleFor(x => x.Plate)
            .Must(x => Regex.IsMatch(x!, PresentationConsts.plateExpression))
            .WithName(Title.Plate)
            .WithMessage(Phrases.InvalidPlate)
            .When(x => x.Plate is not null);

        RuleFor(x => x.Filter)
            .IsInEnum()
            .WithName(Title.Filter)
            .WithMessage(Phrases.InvalidFilter);

    }
}
