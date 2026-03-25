using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Validations.Hub;

public class StartTripHubRequestValidator : AbstractValidator<StartTripHubRequest>
{
    public StartTripHubRequestValidator()
    {
        RuleFor(x => x.BusId)
            .GreaterThan(0)
            .WithName(Title.BusId)
            .WithMessage(Phrases.InvalidBusId);
    }
}
