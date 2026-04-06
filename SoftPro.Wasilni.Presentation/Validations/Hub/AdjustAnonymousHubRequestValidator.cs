using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Validations.Hub;

public class AdjustAnonymousHubRequestValidator : AbstractValidator<AdjustAnonymousHubRequest>
{
    public AdjustAnonymousHubRequestValidator()
    {
        RuleFor(x => x.Delta)
            .GreaterThan(0)
            .WithName(Title.Delta)
            .WithMessage(Phrases.InvalidDelta);
    }
}
