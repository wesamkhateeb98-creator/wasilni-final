using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Booking;

namespace SoftPro.Wasilni.Presentation.Validations.Booking;

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

        RuleFor(x => x.key)
            .NotEqual(Guid.Empty)
            .WithMessage(Phrases.InvalidKey);
    }
}
