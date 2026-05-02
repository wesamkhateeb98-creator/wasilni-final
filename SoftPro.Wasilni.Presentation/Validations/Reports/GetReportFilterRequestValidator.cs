using Domain.Resources;
using FluentValidation;
using SoftPro.Wasilni.Presentation.Models.Request.Report;

namespace SoftPro.Wasilni.Presentation.Validations.Reports;

public class GetReportFilterRequestValidator : AbstractValidator<GetBookingReportRequest>
{
    public GetReportFilterRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(Phrases.InvalidReportType);

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage(Phrases.InvalidDateRange);
    }
}
