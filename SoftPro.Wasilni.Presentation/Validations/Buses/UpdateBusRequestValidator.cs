using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Buses;

public class UpdateBusRequestValidator : AbstractValidator<UpdateBusRequest>
{
    public UpdateBusRequestValidator()
    {
        RuleFor(x => x.Plate)
            .Must(x => Regex.IsMatch(x, PresentationConsts.plateExpression))
            .WithName(Title.Plate)
            .WithMessage(Phrases.InvalidPlate);

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithName(Title.Color)
            .WithMessage(Phrases.InvalidColor);

        RuleFor(x => x.LineId)
            .GreaterThan(0)
            .WithName(Title.LineId)
            .WithMessage(Phrases.InvalidLineId);

        RuleFor(x => x.EstimatedTime)
            .InclusiveBetween(TimeSpan.Zero, new TimeSpan(23, 59, 59))
            .WithName(Title.EstimatedTime)
            .WithMessage(Phrases.InvalidEstimatedTime);
    }
}
