using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Account;

namespace SoftPro.Wasilni.Presentation.Validations.Accounts;

public class GetUsersRequestValidator : AbstractValidator<GetUsersRequest>
{
    public GetUsersRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithName(Title.PageNumber)
            .WithMessage(Phrases.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithName(Title.PageSize)
            .WithMessage(Phrases.InvalidPageSize);
    }
}
