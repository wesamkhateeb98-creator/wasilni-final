using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Trip;

namespace SoftPro.Wasilni.Presentation.Validations.Trips;

public class AddBookingRequestValidator : AbstractValidator<AddBookingRequest>
{
    public AddBookingRequestValidator()
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
