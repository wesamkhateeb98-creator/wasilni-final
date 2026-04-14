using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Booking;

namespace SoftPro.Wasilni.Presentation.Validations.Booking;

public class GetAdminBookingsRequestValidator : AbstractValidator<GetAdminBookingsRequest>
{
    public GetAdminBookingsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithName(Title.PageNumber)
            .WithMessage(Phrases.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithName(Title.PageSize);

        RuleFor(x => x.LineId)
            .GreaterThan(0)
            .WithName(Title.LineId)
            .WithMessage(Phrases.InvalidLineId)
            .When(x => x.LineId is not null);
    }
}
