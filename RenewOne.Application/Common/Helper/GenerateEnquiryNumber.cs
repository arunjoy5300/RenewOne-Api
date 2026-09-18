using RenewOne.Application.Interfaces.Repositories.Enquiry;

namespace RenewOne.Application.Common.Helper
{
    public class GenerateEnquiryNumber
    {
        private readonly IEnquiryRepository _enquiryRepository;
        public GenerateEnquiryNumber(IEnquiryRepository enquiryRepository)
        {
            _enquiryRepository = enquiryRepository;
        }
        public static string GenerateEnquiryNumberAsync()
        {
            var number = Random.Shared.Next(1, 100000);

            return $"ENQ-{number:D4}";
        }
    }
}
