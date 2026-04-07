using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;


namespace SoftPro.Wasilni.Presentation.Validations.Buses;

public class GetBusesRequestValidator : AbstractValidator<GetBusesRequest>
{
    public GetBusesRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithName(Title.PageNumber)
            .WithMessage(Phrases.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithName(Title.PageSize)
            .WithMessage(Phrases.InvalidPageSize);

        RuleFor(x => x.Filter)
            .IsInEnum()
            .WithName(Title.Filter)
            .WithMessage(Phrases.InvalidFilter);
    }
}
