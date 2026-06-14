using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;

namespace RenewOne.Application.LogicService.Enquiry
{
    public class EnquiryLogicService: IEnquiryLogicService
    {
        private readonly IEnquiryRepository _enquiryRepository;
        public EnquiryLogicService(IEnquiryRepository enquiryRepository)
        {
            _enquiryRepository = enquiryRepository;
        }
        public async Task<Result<EnquiryDetailsResponse>> GetEnquiryAsync(Guid enquiryId)
        {
            var enquiry = await _enquiryRepository.GetByIdAsync(enquiryId);

            if (enquiry == null)
            {
                return Result<EnquiryDetailsResponse>.NotFound("Enquiry not found");
            }

            var response = new EnquiryDetailsResponse
            {
                Id = enquiry.Id,
                CustomerName = enquiry.CustomerName,
                CustomerMobile = enquiry.CustomerNumber,
                Status = enquiry.Status,
                PolicyType = enquiry.VehicleEnquiryDetails?.MIPolicyType,

                VehicleNumber = enquiry.VehicleEnquiryDetails?
                    .VehicleDetails?.VehicleNumber,

                ManufacturerName = enquiry.VehicleEnquiryDetails?
                    .VehicleDetails?.ManufacturerName,

                Model = enquiry.VehicleEnquiryDetails?
                    .VehicleDetails?.Model,

                FuelType = enquiry.VehicleEnquiryDetails?
                    .VehicleDetails?.FuelType,

                CubicCapacity = enquiry.VehicleEnquiryDetails?
                    .VehicleDetails?.CubicCapacity,

                CreatedAt = enquiry.CreatedAt,
                UpdatedAt = enquiry.UpdatedAt,

                DocumentsCount = enquiry.Documents.Count,
                QuotesCount = enquiry.EnquiryQuotes.Count,

                Documents = enquiry.Documents
                    .Select(d => new DocumentDto
                    {
                        Id = d.Id,
                        DocType = d.DocType,
                        DocSide = d.DocSide,
                        Url = d.Url
                    })
                    .ToList(),

                Quotes = enquiry.EnquiryQuotes
                    .Select(q => new QuoteDto
                    {
                        Id = q.Id,
                        InsuranceCompanyName = q.InsuranceCompany.CompanyName,
                        Premium = q.Premium,
                        IDV = q.Idv,
                        QuoteDate = q.CreatedAt,
                        AgentPoints = q.AgentPoints,
                        Remarks = q.Remarks
                    })
                    .ToList()
            };

            return Result<EnquiryDetailsResponse>.Success(response);
        }
    }
}
