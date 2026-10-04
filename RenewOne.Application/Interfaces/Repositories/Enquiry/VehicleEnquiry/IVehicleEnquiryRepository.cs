using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry
{
    public interface IVehicleEnquiryRepository
    {
        Task<VehicleType?> GetVehicleTypeByCodeAsync(Guid typeCode);
        Task<VehicleDetails?> GetVehicleDetailsByNumberAsync(string vehicleNumber);
        Task AddVehicleDetailsAsync(VehicleDetails vehicleDetails);
        Task AddEnquiryAsync(Domain.Entities.Enquiry enquiry);
        Task AddVehicleEnquiryDetailsAsync(VehicleEnquiryDetails vehicleEnquiryDetails);
        Task<bool> SaveChangesAsync();
        IQueryable<VehicleDetails> GetVehicleDetailsQuery();
    }
}
