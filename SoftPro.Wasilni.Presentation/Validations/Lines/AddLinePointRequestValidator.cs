using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Line;

namespace SoftPro.Wasilni.Presentation.Validations.Lines;

public class AddLinePointRequestValidator : AbstractValidator<AddLinePointRequest>
{
    public AddLinePointRequestValidator()
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
