using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;

namespace SoftPro.Wasilni.Presentation.Validations.Buses;

public class UpdateLocationRequestValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationRequestValidator()
    {
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
