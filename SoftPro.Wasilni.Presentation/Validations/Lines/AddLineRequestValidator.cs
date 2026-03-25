using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Line;

namespace SoftPro.Wasilni.Presentation.Validations.Lines;

public class AddLineRequestValidator : AbstractValidator<AddLineRequest>
{
    public AddLineRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 50)
            .WithName(Title.LineName)
            .WithMessage(Phrases.InvalidLineName);

        RuleFor(x => x.Points)
            .NotNull()
            .NotEmpty()
            .WithName(Title.Points)
            .WithMessage(Phrases.PointsRequired);

        RuleForEach(x => x.Points)
            .ChildRules(point =>
            {
                point.RuleFor(p => p.Latitude)
                    .InclusiveBetween(-90, 90)
                    .WithName(Title.Latitude)
                    .WithMessage(Phrases.InvalidLatitude);

                point.RuleFor(p => p.Longitude)
                    .InclusiveBetween(-180, 180)
                    .WithName(Title.Longitude)
                    .WithMessage(Phrases.InvalidLongitude);
            });
    }
}
