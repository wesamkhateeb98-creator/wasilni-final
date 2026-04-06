using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;

namespace SoftPro.Wasilni.Presentation.Validations.Buses;

public class AdjustAnonymousRequestValidator : AbstractValidator<AdjustAnonymousRequest>
{
    public AdjustAnonymousRequestValidator()
    {
        RuleFor(x => x.Delta)
            .GreaterThan(0)
            .WithName(Title.Delta)
            .WithMessage(Phrases.InvalidDelta);
    }
}
