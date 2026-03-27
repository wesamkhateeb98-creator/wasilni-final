using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Validations.Hub;

public class AdjustAnonymousHubRequestValidator : AbstractValidator<AdjustAnonymousHubRequest>
{
    public AdjustAnonymousHubRequestValidator()
    {
        RuleFor(x => x.Delta)
            .Must(d => d == 1 || d == -1)
            .WithName(Title.Delta)
            .WithMessage(Phrases.InvalidDelta);
    }
}
