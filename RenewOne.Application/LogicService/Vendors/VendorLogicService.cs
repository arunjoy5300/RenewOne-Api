using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Vendor;
using RenewOne.Application.DTOs.Response.Vendor;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Vendors;
using RenewOne.Application.Interfaces.Repositories.Vendor;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.Vendors
{
    public class VendorLogicService : IVendorLogicService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IUnitOfWork _unitOfWork;
        public VendorLogicService(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
        {
            _vendorRepository = vendorRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<VendorResponse>> CreateVendorAsync(CreateVendorRequest request,CancellationToken ct)
        {

            var lastVendorCode = await _vendorRepository
                .GetQueryable()
                .OrderByDescending(x => x.Code)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(ct);

            var nextNumber = 1;

            if (!string.IsNullOrEmpty(lastVendorCode))
            {
                var numberPart = lastVendorCode.Replace("VEN-", "");

                if (int.TryParse(numberPart, out var number))
                {
                    nextNumber = number + 1;
                }
            }

            var vendorCode = $"VEN-{nextNumber:D4}";


            var vendor = new Vendor
            {
                Name = request.Name.Trim(),
                Code = vendorCode,
                Phone = request.Phone.Trim(),
                Email = request.Email.Trim(),
                BankAccountId = request.BankAccountId,
                IsActive = true
            };

            await _vendorRepository.AddAsync(vendor, ct);
            await _unitOfWork.SaveChangesAsync();

            var response = new VendorResponse
            {
                Id = vendor.Id,
                Name = vendor.Name,
                Code = vendor.Code,
                Phone = vendor.Phone,
                Email = vendor.Email,
                IsActive = vendor.IsActive
            };

            return Result<VendorResponse>.Success(
                response,
                "Vendor created successfully.");
        }

        public async Task<Result<List<VendorResponse>>> GetVendorsAsync(CancellationToken ct)
        {
            var vendors = await _vendorRepository
                .GetQueryable()
                .OrderBy(x => x.Name)
                .Select(x => new VendorResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Phone = x.Phone,
                    Email = x.Email,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);

            return Result<List<VendorResponse>>.Success(
                vendors,
                "Vendors fetched successfully.");
        }

        public async Task<Result<VendorResponse>> GetVendorAsync(Guid id, CancellationToken ct)
        {
            var vendor = await _vendorRepository
                .GetQueryable()
                .Where(x => x.Id == id)
                .Select(x => new VendorResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Phone = x.Phone,
                    Email = x.Email,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);

            if (vendor == null)
            {
                return Result<VendorResponse>.NotFound(
                    "Vendor not found.");
            }

            return Result<VendorResponse>.Success(
                vendor,
                "Vendor fetched successfully.");
        }
        public async Task<Result<VendorResponse>> UpdateVendorAsync(Guid id,UpdateVendorRequest request,CancellationToken ct)
        {
            var vendor = await _vendorRepository
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (vendor == null)
            {
                return Result<VendorResponse>.NotFound(
                    "Vendor not found.");
            }

            var codeExists = await _vendorRepository
                .GetQueryable()
                .AnyAsync(
                    x => x.Code == request.Code && x.Id != id,
                    ct);

            if (codeExists)
            {
                return Result<VendorResponse>.Failure(
                    "Vendor code already exists.", 400);
            }

            vendor.Name = request.Name.Trim();
            vendor.Code = request.Code.Trim();
            vendor.Phone = request.Phone.Trim();
            vendor.Email = request.Email.Trim();
            vendor.IsActive = request.IsActive;
            vendor.UpdatedAt = DateTime.UtcNow;

            _vendorRepository.Update(vendor);
            await _unitOfWork.SaveChangesAsync();

            return Result<VendorResponse>.Success(
                new VendorResponse
                {
                    Id = vendor.Id,
                    Name = vendor.Name,
                    Code = vendor.Code,
                    Phone = vendor.Phone,
                    Email = vendor.Email,
                    IsActive = vendor.IsActive
                },
                "Vendor updated successfully.");
        }

        public async Task<Result<bool>> ToggleVendorStatusAsync(Guid id,CancellationToken ct)
        {
            var vendor = await _vendorRepository
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (vendor == null)
            {
                return Result<bool>.Failure(
                    "Vendor not found.", 404);
            }

            vendor.IsActive = !vendor.IsActive;
            vendor.UpdatedAt = DateTime.UtcNow;

            _vendorRepository.Update(vendor);

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(
                vendor.IsActive,
                vendor.IsActive
                    ? "Vendor activated successfully."
                    : "Vendor deactivated successfully.");
        }
    }
}
