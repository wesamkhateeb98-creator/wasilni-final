using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Line;

namespace SoftPro.Wasilni.Presentation.Validations.Lines;

public class GetLinesRequestValidator : AbstractValidator<GetLinesRequest>
{
    public GetLinesRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithName(Title.PageNumber)
            .WithMessage(Phrases.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithName(Title.PageSize);

        RuleFor(x => x.Name)
            .MinimumLength(1)
                .WithName(Title.LineName)
                .WithMessage(Phrases.InvalidLineName)
            .When(x => x.Name is not null);
    }
}
