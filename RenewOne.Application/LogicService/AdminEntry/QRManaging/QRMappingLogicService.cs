using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.QRMapping;
using RenewOne.Application.DTOs.Response.QRMapping;
using RenewOne.Application.Filters.QRMapping;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.AdminEntry.QRManaging
{
    public class QRMappingLogicService : IQRMappingLogicService
    {
        private readonly IQRMappingRepository _qrMappingRepository;
        private readonly IQRMasterRepository _qrMasterRepository;
        private readonly ILogger<IQRMappingLogicService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInsuranceTypeRepository _insuranceTypeRepository;
        private readonly IVehicleTypeRepository _vehicleTypeRepository;
        public QRMappingLogicService(IQRMappingRepository qrMappingRepository, IQRMasterRepository qrMasterRepository, ILogger<IQRMappingLogicService> logger, IUnitOfWork unitOfWork, IInsuranceTypeRepository insuranceTypeRepository, IVehicleTypeRepository vehicleTypeRepository)
        {
            _qrMappingRepository = qrMappingRepository;
            _qrMasterRepository = qrMasterRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _insuranceTypeRepository = insuranceTypeRepository;
            _vehicleTypeRepository = vehicleTypeRepository;
        }
        public async Task<Result<Guid>> CreateAsync(
        CreateQRMappingRequest request,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating QR Mapping.");

                // Validate QR Master
                var qrMaster = await _qrMasterRepository.GetByIdAsync(request.QRMasterId);

                if (qrMaster == null)
                {
                    _logger.LogWarning("QR Master not found. Id: {Id}", request.QRMasterId);
                    return Result<Guid>.NotFound("QR Master not found.");
                }

                if (!qrMaster.IsActive)
                {
                    _logger.LogWarning("Selected QR Master is inactive. Id: {Id}", request.QRMasterId);
                    return Result<Guid>.Failure("Selected QR Master is inactive.");
                }

                // Validate Insurance Type
                var insuranceType = await _insuranceTypeRepository.GetByIdAsync(request.InsuranceTypeId);

                if (insuranceType == null)
                {
                    _logger.LogWarning("Insurance type not found. Id: {Id}", request.InsuranceTypeId);
                    return Result<Guid>.NotFound("Insurance type not found.");
                }

                // Vehicle insurance requires Vehicle Type
                if (insuranceType.Name.Contains("Vehicle", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.VehicleTypeId.HasValue)
                    {
                        return Result<Guid>.Failure(
                            "Vehicle Type is mandatory for Vehicle Insurance.");
                    }

                    var vehicleType = await _vehicleTypeRepository.GetByIdAsync(
                        request.VehicleTypeId.Value);

                    if (vehicleType == null)
                    {
                        _logger.LogWarning("Vehicle Type not found. Id: {Id}", request.VehicleTypeId.Value);
                        return Result<Guid>.NotFound("Vehicle Type not found.");
                    }
                }
                else
                {
                    // Health, Life, Travel...
                    if (request.VehicleTypeId.HasValue)
                    {
                        _logger.LogWarning("Vehicle Type should not be provided for this Insurance Type. Id: {Id}", request.VehicleTypeId.Value);
                        return Result<Guid>.Failure(
                            "Vehicle Type should not be provided for this Insurance Type.");
                    }
                }

                // Duplicate Mapping Check
                var duplicate = await _qrMappingRepository
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.InsuranceTypeId == request.InsuranceTypeId &&
                        x.VehicleTypeId == request.VehicleTypeId,
                        cancellationToken);

                if (duplicate)
                {
                    return Result<Guid>.Failure(
                        "QR Mapping already exists for the selected Insurance Type and Vehicle Type.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var mapping = new QRMapping
                {
                    Id = Guid.NewGuid(),
                    QRMasterId = request.QRMasterId,
                    InsuranceTypeId = request.InsuranceTypeId,
                    VehicleTypeId = request.VehicleTypeId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _qrMappingRepository.AddAsync(mapping);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "QR Mapping created successfully. Id: {Id}",
                    mapping.Id);

                return Result<Guid>.Success(
                    mapping.Id,
                    "QR Mapping created successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(ex,
                    "Error creating QR Mapping.");

                return Result<Guid>.Failure(
                    "An error occurred while creating QR Mapping.",
                    500);
            }
        }

        public async Task<Result<PagedResponse<QRMappingResponse>>> GetAllAsync(
        QRMappingFilter filter,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching QR Mappings.");

                var query = _qrMappingRepository.GetQueryable();

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                        x.QRMaster.UpiName.Contains(filter.Search) ||
                        x.QRMaster.BankAccount.BankName.Contains(filter.Search) ||
                        x.InsuranceType.Name.Contains(filter.Search) ||
                        (x.VehicleType != null && x.VehicleType.TypeName.Contains(filter.Search)));
                }

                if (filter.QRMasterId.HasValue)
                {
                    query = query.Where(x => x.QRMasterId == filter.QRMasterId);
                }

                if (filter.InsuranceTypeId.HasValue)
                {
                    query = query.Where(x => x.InsuranceTypeId == filter.InsuranceTypeId);
                }

                if (filter.VehicleTypeId.HasValue)
                {
                    query = query.Where(x => x.VehicleTypeId == filter.VehicleTypeId);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == filter.IsActive);
                }

                query = filter.SortDescending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);

                var response = query.Select(x => new QRMappingResponse
                {
                    Id = x.Id,
                    QRMasterId = x.QRMasterId,
                    QRName = x.QRMaster.UpiName,

                    InsuranceTypeId = x.InsuranceTypeId,
                    InsuranceType = x.InsuranceType.Name,

                    VehicleTypeId = x.VehicleTypeId,
                    VehicleType = x.VehicleType != null
                        ? x.VehicleType.TypeName
                        : null,

                    BankName = x.QRMaster.BankAccount.BankName,

                    IsActive = x.IsActive
                });

                var pagedResult = await response.ToPagedResponseAsync(
                    filter.PageNumber,
                    filter.PageSize);

                _logger.LogInformation("QR Mappings fetched successfully.");

                return Result<PagedResponse<QRMappingResponse>>.Success(
                    pagedResult,
                    "QR Mappings fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching QR Mappings.");

                return Result<PagedResponse<QRMappingResponse>>.Failure(
                    "An error occurred while fetching QR Mappings.",
                    500);
            }
        }

        public async Task<Result<QRMappingResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching QR Mapping. Id: {Id}",
                    id);

                var mapping = await _qrMappingRepository.GetByIdAsync(id);

                if (mapping == null)
                {
                    _logger.LogWarning(
                        "QR Mapping not found. Id: {Id}",
                        id);

                    return Result<QRMappingResponse>.NotFound(
                        "QR Mapping not found.");
                }

                var response = new QRMappingResponse
                {
                    Id = mapping.Id,

                    QRMasterId = mapping.QRMasterId,
                    QRName = mapping.QRMaster.UpiName,

                    InsuranceTypeId = mapping.InsuranceTypeId,
                    InsuranceType = mapping.InsuranceType.Name,

                    VehicleTypeId = mapping.VehicleTypeId,
                    VehicleType = mapping.VehicleType?.TypeName,

                    BankName = mapping.QRMaster.BankAccount.BankName,

                    IsActive = mapping.IsActive
                };

                _logger.LogInformation(
                    "QR Mapping fetched successfully. Id: {Id}",
                    id);

                return Result<QRMappingResponse>.Success(
                    response,
                    "QR Mapping fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching QR Mapping. Id: {Id}",
                    id);

                return Result<QRMappingResponse>.Failure(
                    "An error occurred while fetching QR Mapping.",
                    500);
            }
        }

        public async Task<Result<string>> UpdateAsync(
        UpdateQRMappingRequest request,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Updating QR Mapping. Id: {Id}",
                    request.Id);

                var mapping = await _qrMappingRepository.GetByIdAsync(request.Id);

                if (mapping == null)
                {
                    _logger.LogWarning(
                        "QR Mapping not found. Id: {Id}",
                        request.Id);

                    return Result<string>.NotFound("QR Mapping not found.");
                }

                var qrMaster = await _qrMasterRepository.GetByIdAsync(request.QRMasterId);

                if (qrMaster == null)
                {
                    return Result<string>.NotFound("QR Master not found.");
                }

                if (!qrMaster.IsActive)
                {
                    return Result<string>.Failure("Selected QR Master is inactive.");
                }

                var insuranceType = await _insuranceTypeRepository
                    .GetByIdAsync(request.InsuranceTypeId);

                if (insuranceType == null)
                {
                    return Result<string>.NotFound("Insurance Type not found.");
                }

                if (insuranceType.Name.Equals("Vehicle", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.VehicleTypeId.HasValue)
                    {
                        return Result<string>.Failure(
                            "Vehicle Type is mandatory for Vehicle Insurance.");
                    }

                    var vehicleType = await _vehicleTypeRepository
                        .GetByIdAsync(request.VehicleTypeId.Value);

                    if (vehicleType == null)
                    {
                        return Result<string>.NotFound("Vehicle Type not found.");
                    }
                }
                else
                {
                    if (request.VehicleTypeId.HasValue)
                    {
                        return Result<string>.Failure(
                            "Vehicle Type should not be provided for this Insurance Type.");
                    }
                }

                var duplicate = await _qrMappingRepository
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.Id != request.Id &&
                        x.InsuranceTypeId == request.InsuranceTypeId &&
                        x.VehicleTypeId == request.VehicleTypeId,
                        cancellationToken);

                if (duplicate)
                {
                    return Result<string>.Failure(
                        "A QR Mapping already exists for the selected Insurance Type and Vehicle Type.");
                }

                await _unitOfWork.BeginTransactionAsync();

                mapping.QRMasterId = request.QRMasterId;
                mapping.InsuranceTypeId = request.InsuranceTypeId;
                mapping.VehicleTypeId = request.VehicleTypeId;
                mapping.IsActive = request.IsActive;
                mapping.UpdatedAt = DateTime.UtcNow;

                _qrMappingRepository.Update(mapping);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "QR Mapping updated successfully. Id: {Id}",
                    mapping.Id);

                return Result<string>.Success(
                    mapping.Id.ToString(),
                    "QR Mapping updated successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error updating QR Mapping. Id: {Id}",
                    request.Id);

                return Result<string>.Failure(
                    "An error occurred while updating QR Mapping.",
                    500);
            }
        }

        public async Task<Result<string>> ChangeStatusAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Changing QR Mapping status. Id: {Id}, IsActive: {IsActive}",
                    id,
                    isActive);

                var mapping = await _qrMappingRepository.GetByIdAsync(id);

                if (mapping == null)
                {
                    _logger.LogWarning(
                        "QR Mapping not found. Id: {Id}",
                        id);

                    return Result<string>.NotFound(
                        "QR Mapping not found.");
                }

                if (mapping.IsActive == isActive)
                {
                    return Result<string>.Failure(
                        $"QR Mapping is already {(isActive ? "Active" : "Inactive")}.");
                }

                await _unitOfWork.BeginTransactionAsync();

                mapping.IsActive = isActive;
                mapping.UpdatedAt = DateTime.UtcNow;

                _qrMappingRepository.Update(mapping);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "QR Mapping status updated successfully. Id: {Id}",
                    id);

                return Result<string>.Success(
                    id.ToString(),
                    $"QR Mapping {(isActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error changing QR Mapping status. Id: {Id}",
                    id);

                return Result<string>.Failure(
                    "An error occurred while updating QR Mapping status.",
                    500);
            }
        }
    }
}
