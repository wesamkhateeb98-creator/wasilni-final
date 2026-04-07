using Domain.Resources;
using FluentValidation;
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
            .LessThanOrEqualTo(50)
            .WithName(Title.PageSize)
            .WithMessage(Phrases.InvalidPageSize);

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithName(Title.BusType)
            .WithMessage(Phrases.InvalidBusType);
    }
}
