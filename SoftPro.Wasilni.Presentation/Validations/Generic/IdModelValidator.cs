using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;


namespace SoftPro.Wasilni.Presentation.Validations.Generic;

public class IdModelValidator : AbstractValidator<IdRequest>
{
    public IdModelValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithName(Title.Id)
            .WithMessage(Phrases.InvalidId);

    }
}