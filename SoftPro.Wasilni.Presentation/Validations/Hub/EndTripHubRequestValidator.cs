using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Validations.Hub;

public class EndTripHubRequestValidator : AbstractValidator<EndTripHubRequest>
{
    public EndTripHubRequestValidator()
    {
        RuleFor(x => x.TripId)
            .GreaterThan(0)
            .WithName(Title.TripId)
            .WithMessage(Phrases.InvalidTripId);
    }
}
