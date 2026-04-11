using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;
using System.Text.RegularExpressions;

namespace SoftPro.Wasilni.Presentation.Validations.Buses;

public class AddBusRequestValidator : AbstractValidator<AddBusRequest>
{
    public AddBusRequestValidator()
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
            .When(x => x.LineId.HasValue)
            .WithName(Title.LineId)
            .WithMessage(Phrases.InvalidLineId);

        RuleFor(x => x.key)
            .NotEqual(Guid.Empty)
            .WithMessage(Phrases.InvalidKey);
    }
}
