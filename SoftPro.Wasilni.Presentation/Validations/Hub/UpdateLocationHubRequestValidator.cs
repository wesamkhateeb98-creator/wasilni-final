using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Validations.Hub;

public class UpdateLocationHubRequestValidator : AbstractValidator<UpdateLocationHubRequest>
{
    public UpdateLocationHubRequestValidator()
    {
        RuleFor(x => x.TripId)
            .GreaterThan(0)
            .WithName(Title.TripId)
            .WithMessage(Phrases.InvalidTripId);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithName(Title.Latitude)
            .WithMessage(Phrases.InvalidLatitude);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithName(Title.Longitude)
            .WithMessage(Phrases.InvalidLongitude);
    }
}
