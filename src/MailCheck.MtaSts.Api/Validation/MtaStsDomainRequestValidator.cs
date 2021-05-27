using FluentValidation;
using MailCheck.Common.Util;
using MailCheck.MtaSts.Api.Domain;

namespace MailCheck.MtaSts.Api.Validation
{
    public class MtaStsDomainRequestValidator : AbstractValidator<MtaStsDomainRequest>
    {
        public MtaStsDomainRequestValidator(IDomainValidator domainValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(_ => _.Domain)
                .NotNull()
                .WithMessage("A \"domain\" field is required.")
                .NotEmpty()
                .WithMessage("The \"domain\" field should not be empty.")
                .Must(domainValidator.IsValidDomain)
                .WithMessage("The domains must be be a valid domain");
        }
    }
}
