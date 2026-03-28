using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Line;

namespace SoftPro.Wasilni.Presentation.Validations.Lines;

public class UpdateLineRequestValidator : AbstractValidator<UpdateLineNameRequest>
{
    public UpdateLineRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 50)
            .WithName(Title.LineName)
            .WithMessage(Phrases.InvalidLineName);
    }
}
