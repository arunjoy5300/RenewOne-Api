using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryRepository : IVehicleEnquiryRepository
    {
        private readonly AppDbContext _context;

        public VehicleEnquiryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddEnquiryAsync(Domain.Entities.Enquiry enquiry)
        {
            await _context.Enquiries.AddAsync(enquiry);
        }

        public async Task AddVehicleDetailsAsync(VehicleDetails vehicleDetails)
        {
            await _context.VehicleDetails.AddAsync(vehicleDetails);
        }

        public async Task AddVehicleEnquiryDetailsAsync(VehicleEnquiryDetails vehicleEnquiryDetails)
        {
            await _context.VehicleEnquiryDetails.AddAsync(vehicleEnquiryDetails);
        }

        public async Task<VehicleDetails?> GetVehicleDetailsByNumberAsync(string vehicleNumber)
        {
            return await _context.VehicleDetails
            .FirstOrDefaultAsync(v => v.VehicleNumber == vehicleNumber);
        }

        public Task<VehicleType?> GetVehicleTypeByCodeAsync(Guid typeId)
        {
            return _context.VehicleTypes.FirstOrDefaultAsync(vt => vt.Id == typeId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
