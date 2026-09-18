using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Vendor;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.DTOs.Response.Vendor;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Hubs.RenewOne.Application.Hubs;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.AssignmentHistories;
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
using RenewOne.Application.Interfaces.Repositories.Documents;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.PolicyDetails;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Repositories.Vendor;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Application.Interfaces.Services.SignalR;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace RenewOne.Application.LogicService.Enquiry
{
    public class EnquiryLogicService : IEnquiryLogicService
    {
        #region DI
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<EnquiryLogicService> _logger;
        private readonly IUserTaskAllocationRepository _userTaskAllocationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAssignmentHistoryRepository _assignmentHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IEnquiryQuoteRepository _enquiryQuoteRepository;
        private readonly ILedgerRepository _ledgerRepository;
        private readonly IPolicyPaymentRepository _policyPaymentRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IVendorTransactionRepository _vendorTransactionRepository;
        private readonly IPolicyDetailsRepository _policyDetailsRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IBankTransactionRepository _bankTransactionRepository;
        private readonly IDocumentRepository _documentRepository;
        #endregion

        #region Constructor
        public EnquiryLogicService(
            IEnquiryRepository enquiryRepository,
            ICurrentUserService currentUser,
            ILogger<EnquiryLogicService> logger,
            IUserTaskAllocationRepository userTaskAllocationRepository,
            IUserRepository userRepository,
            IAssignmentHistoryRepository assignmentHistoryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEnquiryQuoteRepository enquiryQuoteRepository,
            ILedgerRepository ledgerRepository,
            IPolicyPaymentRepository policyPaymentRepository,
            IVendorRepository vendorRepository,
            IVendorTransactionRepository vendorTransactionRepository,
            IPolicyDetailsRepository policyDetailsRepository,
            IBankAccountRepository bankAccountRepository,
            IBankTransactionRepository bankTransactionRepository,
            IDocumentRepository documentRepository)
        {
            _enquiryRepository = enquiryRepository;
            _currentUser = currentUser;
            _logger = logger;
            _userTaskAllocationRepository = userTaskAllocationRepository;
            _userRepository = userRepository;
            _assignmentHistoryRepository = assignmentHistoryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _enquiryQuoteRepository = enquiryQuoteRepository;
            _ledgerRepository = ledgerRepository;
            _policyPaymentRepository = policyPaymentRepository;
            _vendorRepository = vendorRepository;
            _vendorTransactionRepository = vendorTransactionRepository;
            _policyDetailsRepository = policyDetailsRepository;
            _bankAccountRepository = bankAccountRepository;
            _bankTransactionRepository = bankTransactionRepository;
            _documentRepository = documentRepository;
        }
        #endregion

        #region MyEnquiryDashboard
        public async Task<Result<EnquiryDashboardResponse>> MyEnquiryDashboard(DashboardFilter filter)
        {
            try
            {
                var userId = _currentUser.UserId;

                var query = _enquiryRepository.GetEnquiryQueryable()
                    .Where(x => x.CreatedByUserId == userId);

                switch (filter.DateFilter)
                {
                    case DateFilterType.Today:
                        query = query.Where(x => x.CreatedAt.Date == DateTime.Today);
                        break;

                    case DateFilterType.Yesterday:
                        var yesterday = DateTime.Today.AddDays(-1);
                        query = query.Where(x => x.CreatedAt.Date == yesterday);
                        break;

                    case DateFilterType.ThisWeek:
                        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        query = query.Where(x => x.CreatedAt.Date >= startOfWeek);
                        break;

                    case DateFilterType.ThisMonth:
                        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        query = query.Where(x => x.CreatedAt.Date >= startOfMonth);
                        break;

                    case DateFilterType.Custom:
                        if (filter.FromDate.HasValue)
                        {
                            var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                            query = query.Where(x => x.CreatedAt >= from);
                        }

                        if (filter.ToDate.HasValue)
                        {
                            var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                            query = query.Where(x => x.CreatedAt <= to);
                        }
                        break;
                }

                var response = await query
                    .GroupBy(x => 1)
                    .Select(g => new EnquiryDashboardResponse
                    {
                        TotalEnquiries = g.Count(),
                        Created = g.Count(x => x.Status == EnquiryStatus.Created.ToString()),
                        PendingAssignment = g.Count(x => x.Status == EnquiryStatus.Submitted.ToString()),
                        Quoted = g.Count(x => x.Status == EnquiryStatus.Quoted.ToString() || x.Status == EnquiryStatus.PolicyInProgress.ToString()),
                        PolicyIssued = g.Count(x => x.Status == EnquiryStatus.PolicyIssued.ToString())
                    })
                    .FirstOrDefaultAsync() ?? new EnquiryDashboardResponse();

                _logger.LogInformation("Dashboard count fetched successfully for user {userId} from IP Address: {ip} : response {@response}", userId, _currentUser.IpAddress, response);

                return Result<EnquiryDashboardResponse>.Success(response, "Dashboard count fetched successfully");
            }
            catch (Exception ex)
            {
                return Result<EnquiryDashboardResponse>
                    .Failure("An error occurred while fetching dashboard count", 500);
            }
        }
        #endregion

        #region AllEnquirieDashboard
        public async Task<Result<EnquiryDashboardResponse>> AllEnquiryDashboard(DashboardFilter filter)
        {
            try
            {
                var accountId = _currentUser.AccountId;

                var query = _enquiryRepository.GetEnquiryQueryable()
                    .Where(x => x.TenantId == accountId);

                switch (filter.DateFilter)
                {
                    case DateFilterType.Today:
                        query = query.Where(x => x.CreatedAt.Date == DateTime.Today);
                        break;

                    case DateFilterType.Yesterday:
                        var yesterday = DateTime.Today.AddDays(-1);
                        query = query.Where(x => x.CreatedAt.Date == yesterday);
                        break;

                    case DateFilterType.ThisWeek:
                        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        query = query.Where(x => x.CreatedAt.Date >= startOfWeek);
                        break;

                    case DateFilterType.ThisMonth:
                        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        query = query.Where(x => x.CreatedAt.Date >= startOfMonth);
                        break;

                    case DateFilterType.Custom:
                        if (filter.FromDate.HasValue)
                        {
                            var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                            query = query.Where(x => x.CreatedAt >= from);
                        }

                        if (filter.ToDate.HasValue)
                        {
                            var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                            query = query.Where(x => x.CreatedAt <= to);
                        }
                        break;
                }

                var response = await query
                    .GroupBy(x => 1)
                    .Select(g => new EnquiryDashboardResponse
                    {
                        TotalEnquiries = g.Count(),
                        Created = g.Count(x => x.Status == EnquiryStatus.Created.ToString()),
                        PendingAssignment = g.Count(x => x.Status == EnquiryStatus.Submitted.ToString()),
                        Quoted = g.Count(x => x.Status == EnquiryStatus.Quoted.ToString() || x.Status == EnquiryStatus.PolicyInProgress.ToString()),
                        PolicyIssued = g.Count(x => x.Status == EnquiryStatus.PolicyIssued.ToString())
                    })
                    .FirstOrDefaultAsync() ?? new EnquiryDashboardResponse();

                _logger.LogInformation("Dashboard count fetched successfully for user {userId} from IP Address: {ip} : response {@response}", _currentUser.UserId, _currentUser.IpAddress, response);
                return Result<EnquiryDashboardResponse>.Success(response, "Dashboard count fetched successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching dashboard count for user {userId} from IP Address: {ip}", _currentUser.UserId, _currentUser.IpAddress);
                return Result<EnquiryDashboardResponse>
                    .Failure("An error occurred while fetching dashboard count", 500);
            }
        }
        #endregion

        #region GetAllEnquiries
        public async Task<Result<PagedResponse<EnquiryListResponse>>> GetEnquiriesAsync(EnquiryFilter filter)
        {
            try
            {
                _logger.LogInformation("Fetching enquiries with filter: {@filter} from IP Address: {ip}", filter, _currentUser.IpAddress);

                var accountId = _currentUser.AccountId;

                var query = _enquiryRepository
                    .GetEnquiryQueryable()
                    .Where(x => x.TenantId == accountId);

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.Trim();

                    query = query.Where(x =>
                        x.CustomerName.Contains(search) ||
                        x.CustomerNumber.Contains(search));
                }

                if (filter.VehicleTypeId.HasValue)
                {
                    query = query.Where(x =>
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleTypeId ==
                        filter.VehicleTypeId);
                }

                if (filter.PolicyType.HasValue)
                {
                    query = query.Where(x =>
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.MIPolicyType ==
                        filter.PolicyType.Value.ToString());
                }

                if (filter.EnquiryStatus.HasValue)
                {
                    query = query.Where(x =>
                        x.Status == filter.EnquiryStatus.Value.ToString());
                }

                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(x => x.CreatedAt >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(x => x.CreatedAt <= to);
                }

                query = filter.SortBy switch
                {
                    EnquirySortBy.CreatedAt =>
                        query.OrderByDescending(x => x.CreatedAt),

                    EnquirySortBy.CustomerName =>
                        query.OrderBy(x => x.CustomerName),

                    _ => query.OrderByDescending(x => x.CreatedAt)
                };

                var response = query.Select(x => new EnquiryListResponse
                {
                    EnquiryId = x.Id,

                    CustomerName = x.CustomerName,

                    ContactNumber = x.CustomerNumber,

                    Status = x.Status,

                    VehicleType = x.VehicleEnquiryDetails != null
                        ? x.VehicleEnquiryDetails.VehicleType.TypeName
                        : null,

                    PolicyType = x.VehicleEnquiryDetails != null
                        ? x.VehicleEnquiryDetails.MIPolicyType
                        : null,

                    VehicleNumber = x.VehicleEnquiryDetails != null
                        ? x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                        : null,

                    CreatedByName = x.CreatedByUserId != null ?
                          x.CreatedByUser.FullName
                        : null,

                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                });

                var pagedResult = await response.ToPagedResponseAsync(
                    filter.PageNumber,
                    filter.PageSize);

                return Result<PagedResponse<EnquiryListResponse>>
                    .Success(pagedResult);

            }
            catch (Exception ex)
            {
                return Result<PagedResponse<EnquiryListResponse>>
                    .Failure("An error occurred while fetching enquiries", 500);
            }
        }
        #endregion

        #region GetMyEnquiries
        public async Task<Result<PagedResponse<EnquiryListResponse>>> GetMyEnquiriesAsync(EnquiryFilter filter)
        {
            try
            {
                _logger.LogInformation("Request received from IP Address to fetch MyEnquiries: {ip}", _currentUser.IpAddress);
                var accountId = _currentUser.AccountId;
                var userId = _currentUser.UserId;

                var query = _enquiryRepository
                    .GetEnquiryQueryable()
                    .Where(x => x.TenantId == accountId &&
                                x.CreatedByUserId == userId);

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.Trim();

                    query = query.Where(x =>
                        x.CustomerName.Contains(search) ||
                        x.CustomerNumber.Contains(search));
                }

                if (filter.VehicleTypeId.HasValue)
                {
                    query = query.Where(x =>
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleTypeId ==
                        filter.VehicleTypeId);
                }

                if (filter.PolicyType.HasValue)
                {
                    query = query.Where(x =>
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.MIPolicyType ==
                        filter.PolicyType.Value.ToString());
                }

                if (filter.EnquiryStatus.HasValue)
                {
                    query = query.Where(x =>
                        x.Status == filter.EnquiryStatus.Value.ToString());
                }

                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);

                    query = query.Where(x => x.CreatedAt >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);

                    query = query.Where(x => x.CreatedAt <= to);
                }

                query = filter.SortBy switch
                {
                    EnquirySortBy.CreatedAt =>
                        query.OrderByDescending(x => x.CreatedAt),

                    EnquirySortBy.CustomerName =>
                        query.OrderBy(x => x.CustomerName),

                    _ => query.OrderByDescending(x => x.CreatedAt)
                };

                var response = query.Select(x => new EnquiryListResponse
                {
                    EnquiryId = x.Id,

                    CustomerName = x.CustomerName,

                    ContactNumber = x.CustomerNumber,

                    Status = x.Status,

                    VehicleType = x.VehicleEnquiryDetails != null
                        ? x.VehicleEnquiryDetails.VehicleType.TypeName
                        : null,

                    PolicyType = x.VehicleEnquiryDetails != null
                        ? x.VehicleEnquiryDetails.MIPolicyType
                        : null,

                    VehicleNumber = x.VehicleEnquiryDetails != null
                        ? x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                        : null,

                    CreatedByName = x.CreatedByUserId != null ?
                          x.CreatedByUser.FullName
                        : null,

                    CreatedAt = x.CreatedAt
                });

                var pagedResult = await response.ToPagedResponseAsync(
                    filter.PageNumber,
                    filter.PageSize);

                return Result<PagedResponse<EnquiryListResponse>>
                    .Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while fetching my enquiries {ex}", ex);
                return Result<PagedResponse<EnquiryListResponse>>
                    .Failure("An error occurred while fetching enquiries", 500);
            }
        }
        #endregion

        #region GetEnquiryById
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

                CreatedBy = enquiry.CreatedByUser != null
                    ? enquiry.CreatedByUser.FullName
                    : null,

                AssignedAgent = enquiry.AssignedEmployee != null
                    ? enquiry.AssignedEmployee.FullName
                    : null,

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
                        IsSelected = q.IsSelected,
                        Remarks = q.Remarks
                    })
                    .ToList(),

                RcDetails = new RcApiDto
                {
                    ManufacturerName = enquiry.VehicleEnquiryDetails.VehicleDetails.ManufacturerName,
                    Model = enquiry.VehicleEnquiryDetails.VehicleDetails.Model,
                    FuelType = enquiry.VehicleEnquiryDetails.VehicleDetails.FuelType,
                    CubicCapacity = enquiry.VehicleEnquiryDetails.VehicleDetails.CubicCapacity,
                    BodyType = enquiry.VehicleEnquiryDetails.VehicleDetails.BodyType,
                    ChassisNumber = enquiry.VehicleEnquiryDetails.VehicleDetails.ChassisNumber,
                    EngineNumber = enquiry.VehicleEnquiryDetails.VehicleDetails.EngineNumber,
                    VehicleNumber = enquiry.VehicleEnquiryDetails.VehicleDetails.VehicleNumber,
                    VehicleClass = enquiry.VehicleEnquiryDetails.VehicleDetails.VehicleClass,
                    VehicleColor = enquiry.VehicleEnquiryDetails.VehicleDetails.VehicleColor,
                    NormsType = enquiry.VehicleEnquiryDetails.VehicleDetails.NormsType,
                    VehicleCategory = enquiry.VehicleEnquiryDetails.VehicleDetails.VehicleCategory,
                    GrossVehicleWeight = enquiry.VehicleEnquiryDetails.VehicleDetails.GrossVehicleWeight,
                    UnladenWeight = enquiry.VehicleEnquiryDetails.VehicleDetails.UnladenWeight,
                    Wheelbase = enquiry.VehicleEnquiryDetails.VehicleDetails.Wheelbase,
                    SeatCapacity = enquiry.VehicleEnquiryDetails.VehicleDetails.SeatCapacity,
                    CylinderCount = enquiry.VehicleEnquiryDetails.VehicleDetails.CylinderCount,
                    IsFinanced = enquiry.VehicleEnquiryDetails.VehicleDetails.IsFinanced,
                    OwnerName = enquiry.VehicleEnquiryDetails.VehicleDetails.OwnerName,
                    OwnerFatherName = enquiry.VehicleEnquiryDetails.VehicleDetails.OwnerFatherName,
                    MobileNumber = enquiry.VehicleEnquiryDetails.VehicleDetails.MobileNumber,
                    OwnerCount = enquiry.VehicleEnquiryDetails.VehicleDetails.OwnerCount,
                    RegistrationAuthority = enquiry.VehicleEnquiryDetails.VehicleDetails.RegistrationAuthority,
                    RegistrationDate = enquiry.VehicleEnquiryDetails.VehicleDetails.RegistrationDate,
                    RcExpiryDate = enquiry.VehicleEnquiryDetails.VehicleDetails.RcExpiryDate,
                    ManufacturingMonthYear = enquiry.VehicleEnquiryDetails.VehicleDetails.ManufacturingMonthYear,
                    VehicleTaxUpto = enquiry.VehicleEnquiryDetails.VehicleDetails.VehicleTaxUpto,
                    RtoCode = enquiry.VehicleEnquiryDetails.VehicleDetails.RtoCode,
                    InsuranceCompanyName = enquiry.VehicleEnquiryDetails.VehicleDetails.InsuranceCompanyName,
                    InsurancePolicyNumber = enquiry.VehicleEnquiryDetails.VehicleDetails.InsurancePolicyNumber,
                    InsuranceValidUpto = enquiry.VehicleEnquiryDetails.VehicleDetails.InsuranceValidUpto,
                    FinancerName = enquiry.VehicleEnquiryDetails.VehicleDetails.FinancerName,
                    PuccNumber = enquiry.VehicleEnquiryDetails.VehicleDetails.PuccNumber,
                    PuccUpto = enquiry.VehicleEnquiryDetails.VehicleDetails.PuccUpto,
                    PresentAddress = enquiry.VehicleEnquiryDetails.VehicleDetails.PresentAddress,
                    PermanentAddress = enquiry.VehicleEnquiryDetails.VehicleDetails.PermanentAddress,
                    RawApiResponse = enquiry.VehicleEnquiryDetails.VehicleDetails.RawApiResponse
                }
            };

            return Result<EnquiryDetailsResponse>.Success(response);
        }
        #endregion

        #region Admin - AssignEnquiry
        public async Task<Result<string>> AssignAsync(Guid enquiryId,Guid employeeId,string? remarks,CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Assigning enquiry {EnquiryId} to employee {EmployeeId}",
                    enquiryId,
                    employeeId);

                var enquiry = await _enquiryRepository
                    .GetEnquiryQueryable()
                    .FirstOrDefaultAsync(
                        x => x.Id == enquiryId,
                        cancellationToken);

                if (enquiry == null)
                {
                    _logger.LogWarning(
                        "Enquiry not found. EnquiryId: {EnquiryId}",
                        enquiryId);

                    return Result<string>.NotFound("Enquiry not found.");
                }

                if (enquiry.AssignedEmployeeId == employeeId)
                {
                    return Result<string>.Failure(
                        "Enquiry is already assigned to the selected employee.");
                }

                var employee = await _userRepository.GetByIdAsync(employeeId);

                if (employee == null)
                {
                    _logger.LogWarning(
                        "Employee not found. EmployeeId: {EmployeeId}",
                        employeeId);

                    return Result<string>.NotFound("Employee not found.");
                }

                if (_currentUser.Role != "SuperAdmin")
                {
                    var hasPermission = await _userTaskAllocationRepository
                        .GetQueryable()
                        .AsNoTracking()
                        .AnyAsync(
                            x =>
                                x.UserId == employeeId &&
                                x.InsuranceTypeId == enquiry.InsuranceTypeId,
                            cancellationToken);

                    if (!hasPermission)
                    {
                        _logger.LogWarning(
                            "Employee {EmployeeId} is not authorized for InsuranceType {InsuranceTypeId}",
                            employeeId,
                            enquiry.InsuranceTypeId);

                        return Result<string>.Failure(
                            "Selected employee is not authorized for this insurance type.");
                    }
                }

                var previousEmployeeId = enquiry.AssignedEmployeeId;
                var previousStatus = enquiry.Status;

                var nextStatus = enquiry.Status switch
                {
                    nameof(EnquiryStatus.Submitted) =>
                        nameof(EnquiryStatus.QuoteInProgress),

                    nameof(EnquiryStatus.PaymentVerificationPending) =>
                        nameof(EnquiryStatus.PolicyInProgress),

                    _ => enquiry.Status
                };

                var now = DateTime.UtcNow;

                await _unitOfWork.BeginTransactionAsync();

                var assignmentHistory = new AssignmentHistory
                {
                    Id = Guid.NewGuid(),
                    EnquiryId = enquiry.Id,
                    ChangedByUserId = _currentUser.UserId,
                    FromStatus = previousStatus,
                    ToStatus = nextStatus,
                    FromEmployeeId = previousEmployeeId,
                    ToEmployeeId = employeeId,
                    Remarks = remarks,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                enquiry.AssignedEmployeeId = employeeId;
                enquiry.Status = nextStatus;
                enquiry.UpdatedAt = now;

                _enquiryRepository.Update(enquiry);

                await _assignmentHistoryRepository.AddAsync(
                    assignmentHistory);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Enquiry {EnquiryId} assigned from {PreviousEmployeeId} to {EmployeeId}",
                    enquiryId,
                    previousEmployeeId,
                    employeeId);

                try
                {
                    await _notificationService.SendToUsersAsync(
                        new NotificationMessage
                        {
                            Event = "EnquiryAssigned",
                            Title = previousEmployeeId.HasValue
                                ? "Enquiry Reassigned"
                                : "New Enquiry Assigned",
                            Message = previousEmployeeId.HasValue
                                ? $"Enquiry #{enquiry.Id} has been reassigned to you."
                                : $"Enquiry #{enquiry.Id} has been assigned to you.",
                            ReferenceId = enquiry.Id,
                            Status = enquiry.Status
                        },
                        employeeId);
                }
                catch (Exception notificationException)
                {
                    _logger.LogError(
                        notificationException,
                        "Enquiry {EnquiryId} was assigned successfully, but notification failed for employee {EmployeeId}",
                        enquiryId,
                        employeeId);
                }

                return Result<string>.Success(
                    enquiry.Id.ToString(),
                    previousEmployeeId.HasValue
                        ? "Enquiry reassigned successfully."
                        : "Enquiry assigned successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error assigning enquiry {EnquiryId}",
                    enquiryId);

                return Result<string>.Failure(
                    "An error occurred while assigning the enquiry.",
                    500);
            }
        }
        #endregion

        #region Admin - GetAssignableEmployeesAsync
        public async Task<Result<List<AssignableEmployeeResponse>>> GetAssignableEmployeesAsync(
        Guid enquiryId,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching assignable employees for enquiry {EnquiryId}",
                    enquiryId);

                var enquiry = await _enquiryRepository
                    .GetEnquiryQueryable()
                    .FirstOrDefaultAsync(x => x.Id == enquiryId, cancellationToken);

                if (enquiry == null)
                {
                    return Result<List<AssignableEmployeeResponse>>
                        .NotFound("Enquiry not found.");
                }

                var employees = await _userTaskAllocationRepository
                    .GetQueryable()
                    .Where(x =>
                        x.InsuranceTypeId == enquiry.InsuranceTypeId &&
                        x.IsActive)
                    .Select(x => x.User)
                    .Distinct()
                    .OrderBy(x => x.FullName)
                    .Select(x => new AssignableEmployeeResponse
                    {
                        UserId = x.Id,
                        FullName = x.FullName,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber,
                        IsCurrentlyAssigned = enquiry.AssignedEmployeeId == x.Id
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "{Count} assignable employees found.",
                    employees.Count);

                return Result<List<AssignableEmployeeResponse>>
                    .Success(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching assignable employees.");

                return Result<List<AssignableEmployeeResponse>>
                    .Failure(
                        "An error occurred while fetching assignable employees.",
                        500);
            }
        }
        #endregion

        #region GetEnquiryLedgerDetails
        public async Task<Result<EnquiryLedgerDetails>> GetEnquiryLedgerDetails(Guid enquiryId, CancellationToken ct)
        {
            try
            {
                var enquiry = await _enquiryRepository.GetByIdAsync(enquiryId);
                if (enquiry == null)
                {
                    return Result<EnquiryLedgerDetails>.NotFound("Enquiry not found");
                }

                if (enquiry.SelectedQuoteId == null)
                {
                    return Result<EnquiryLedgerDetails>.Failure("No quote selected for this enquiry");
                }

                var quoteDetials = await _enquiryQuoteRepository.GetEnquiryQuoteQuery()
                    .FirstOrDefaultAsync(x => x.EnquiryId == enquiryId && x.Id == enquiry.SelectedQuoteId && x.IsSelected, ct);

                if (quoteDetials == null)
                {
                    return Result<EnquiryLedgerDetails>.NotFound("No quote found for this enquiry");
                }

                var ledger = await _ledgerRepository.GetByAccountIdAsync(enquiry.TenantId);
                var finalBalance = quoteDetials.Premium - quoteDetials.AgentPoints;

                var response = new EnquiryLedgerDetails
                {
                    LedgerBalance = ledger.AvailableBalance,
                    FinalAmount = finalBalance,
                    BalanceAfterCut = ledger.AvailableBalance - finalBalance
                };

                return Result<EnquiryLedgerDetails>.Success(response, "Ledger details fetched successfully");

            }
            catch (Exception ex)
            {
                return Result<EnquiryLedgerDetails>.Failure($"Error fetching ledger details: {ex.Message}", 500);
            }
        }
        #endregion

        #region Admin - Enquiry - Common Enquiry ( Queue )
        public async Task<Result<PagedResponse<CommonQueueResponse>>> GetCommonQueueAsync(CommonQueueFilter filter, CancellationToken ct)
        {
            var userId = _currentUser.UserId;
            var role = _currentUser.Role;

            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking()
                .Where(e => e.AssignedEmployeeId == null);

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.DateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.ThisYear:
                    fromDate = new DateOnly(
                        today.Year,
                        1,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<PagedResponse<CommonQueueResponse>>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<PagedResponse<CommonQueueResponse>>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                default:
                    return Result<PagedResponse<CommonQueueResponse>>.Failure(
                        "Invalid date filter.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);
            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(e =>
                e.CreatedAt >= from &&
                e.CreatedAt < toExclusive);

            var isPrivilegedUser =
                role == "SuperAdmin" ||
                role == "Supervisor";

            if (!isPrivilegedUser)
            {
                var allocations = _userTaskAllocationRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x =>
                        x.UserId == userId &&
                        x.CanAssign);

                query = query.Where(e =>
                    allocations.Any(a =>
                        a.InsuranceTypeId == e.InsuranceTypeId &&
                        (
                            a.VehicleTypeId == null ||
                            a.VehicleTypeId ==
                                e.VehicleEnquiryDetails!.VehicleTypeId
                        )
                    ));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(e =>
                    e.CustomerName.Contains(search) ||
                    e.CustomerNumber.Contains(search) ||
                    (
                        e.VehicleEnquiryDetails != null &&
                        e.VehicleEnquiryDetails.VehicleDetails != null &&
                        e.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            .Contains(search)
                    ));
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                var insuranceTypeId = filter.InsuranceTypeId.Value;

                query = query.Where(e =>
                    e.InsuranceTypeId == insuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                var vehicleTypeId = filter.VehicleTypeId.Value;

                query = query.Where(e =>
                    e.VehicleEnquiryDetails != null &&
                    e.VehicleEnquiryDetails.VehicleTypeId == vehicleTypeId);
            }

            var totalCount = await query.CountAsync(ct);

            var resultQuery = query
                .Select(e => new CommonQueueResponse
                {
                    Id = e.Id,
                    EnquiryId = e.EnquiryNumber,
                    CustomerName = e.CustomerName,
                    CustomerNumber = e.CustomerNumber,
                    //VehicleNumber = e.VehicleEnquiryDetails!
                    //    .VehicleDetails!
                    //    .VehicleNumber,
                    //VehicleType = e.VehicleEnquiryDetails!
                    //    .VehicleType!
                    //    .TypeName,
                    InsuranceType = e.InsuranceType!
                        .Name,
                    //PolicyType = e.VehicleEnquiryDetails!
                    //    .MIPolicyType,
                    Status = e.Status,
                    CreatedBy = e.CreatedByUser!
                        .FullName,
                    CreatedByPhone = e.CreatedByUser!
                        .PhoneNumber,
                    CreatedByRole = e.CreatedByUser!
                        .UserType,
                    AgencyName = e.Tenant!
                        .AgentName,
                    SubmittedOn = e.UpdatedAt,
                    Priority = e.Priority
                });

            resultQuery = filter.SortDescending
                ? resultQuery
                    .OrderByDescending(x => x.SubmittedOn)
                    .ThenByDescending(x => x.Id)
                : resultQuery
                    .OrderBy(x => x.SubmittedOn)
                    .ThenBy(x => x.Id);

            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var result = await resultQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            var response = new PagedResponse<CommonQueueResponse>
            {
                Items = result,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalCount,
                TotalPages = totalPages
            };

            return Result<PagedResponse<CommonQueueResponse>>.Success(
                response,
                "Common queue fetched successfully.");
        }
        #endregion

        #region Admin - Enquiry - All Equiry
        public async Task<Result<PagedResponse<QuoteQueueResponse>>> GetAllAdminEnquiriesAsync(PolicyQueueFilter filter, CancellationToken ct)
        {
            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking();

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.DateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                case DateFilterType.ThisYear:
                    fromDate = new DateOnly(
                        today.Year,
                        1,
                        1);

                    toDate = today;
                    break;

                default:
                    return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                        "Invalid date filter.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);

            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.CreatedAt >= from &&
                x.CreatedAt < toExclusive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.CustomerName.Contains(search) ||
                    x.CustomerNumber.Contains(search) ||
                    (
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            .Contains(search)
                    ));
            }

            if (filter.Status.HasValue)
            {
                var status = filter.Status.Value.ToString();

                query = query.Where(x =>
                    x.Status == status);
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                var insuranceTypeId = filter.InsuranceTypeId.Value;

                query = query.Where(x =>
                    x.InsuranceTypeId == insuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                var vehicleTypeId = filter.VehicleTypeId.Value;

                query = query.Where(x =>
                    x.VehicleEnquiryDetails != null &&
                    x.VehicleEnquiryDetails.VehicleTypeId == vehicleTypeId);
            }

            if (filter.AccountId.HasValue)
            {
                var accountId = filter.AccountId.Value;

                query = query.Where(x =>
                    x.TenantId == accountId);
            }

            var totalRecords = await query.CountAsync(ct);

            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var skip = (pageNumber - 1) * pageSize;

            var orderedQuery = filter.SortDescending
                ? query
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                : query
                    .OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id);

            var pageIds = await orderedQuery
                .Select(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

            if (pageIds.Count == 0)
            {
                var emptyResponse = new PagedResponse<QuoteQueueResponse>
                {
                    Items = [],
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(
                        (double)totalRecords / pageSize)
                };

                return Result<PagedResponse<QuoteQueueResponse>>.Success(
                    emptyResponse,
                    "Enquiries fetched successfully.");
            }

            var items = await _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking()
                .Where(x => pageIds.Contains(x.Id))
                .Select(x => new QuoteQueueResponse
                {
                    Id = x.Id,
                    EnquiryId = x.EnquiryNumber,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    //VehicleNumber = x.VehicleEnquiryDetails!
                    //    .VehicleDetails!
                    //    .VehicleNumber,
                    //VehicleType = x.VehicleEnquiryDetails!
                    //    .VehicleType!
                    //    .TypeName,
                    InsuranceType = x.InsuranceType!
                        .Name,
                    //PolicyType = x.VehicleEnquiryDetails!
                    //    .MIPolicyType,
                    Status = x.Status,
                    CurrentStep = x.CurrentStep,
                    CreatedBy = x.CreatedByUser!
                        .FullName,
                    CreatedByPhone = x.CreatedByUser!
                        .PhoneNumber,
                    CreatedByRole = x.CreatedByUser!
                        .UserType,
                    AgencyName = x.Tenant!
                        .AgentName,
                    AssignedEmployeeId = x.AssignedEmployeeId,
                    AssignedEmployee = x.AssignedEmployee != null
                        ? x.AssignedEmployee.FullName
                        : null,
                    AssignedEmployeeRole = x.AssignedEmployee != null
                        ? x.AssignedEmployee.UserType
                        : null,
                    AdminAssisted = x.AdminAssisted == true,
                    SubmittedOn = x.CreatedAt
                })
                .ToListAsync(ct);

            var orderedItems = filter.SortDescending
                ? items
                    .OrderByDescending(x => x.SubmittedOn)
                    .ThenByDescending(x => x.Id)
                    .ToList()
                : items
                    .OrderBy(x => x.SubmittedOn)
                    .ThenBy(x => x.Id)
                    .ToList();

            var pagedResult = new PagedResponse<QuoteQueueResponse>
            {
                Items = orderedItems,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(
                    (double)totalRecords / pageSize)
            };

            return Result<PagedResponse<QuoteQueueResponse>>.Success(
                pagedResult,
                "Enquiries fetched successfully.");
        }
        #endregion

        #region Admin - Enquiry - My Enquiry ( enquiries assigned to current logged user )
        public async Task<Result<PagedResponse<CommonQueueResponse>>> MyTaskEnquiries(EnquiryFilter filter,CancellationToken ct)
        {
            var currentUserId = _currentUser.UserId;

            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking()
                .Where(x => x.AssignedEmployeeId == currentUserId);

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.DateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.ThisYear:
                    fromDate = new DateOnly(
                        today.Year,
                        1,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<PagedResponse<CommonQueueResponse>>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<PagedResponse<CommonQueueResponse>>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                default:
                    return Result<PagedResponse<CommonQueueResponse>>.Failure(
                        "Invalid date filter.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);

            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.CreatedAt >= from &&
                x.CreatedAt < toExclusive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.CustomerName.Contains(search) ||
                    x.CustomerNumber.Contains(search) ||
                    (
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            .Contains(search)
                    ));
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                var insuranceTypeId = filter.InsuranceTypeId.Value;

                query = query.Where(x =>
                    x.InsuranceTypeId == insuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                var vehicleTypeId = filter.VehicleTypeId.Value;

                query = query.Where(x =>
                    x.VehicleEnquiryDetails != null &&
                    x.VehicleEnquiryDetails.VehicleTypeId == vehicleTypeId);
            }

            if (filter.AccountId.HasValue)
            {
                var accountId = filter.AccountId.Value;

                query = query.Where(x =>
                    x.TenantId == accountId);
            }

            var totalCount = await query.CountAsync(ct);

            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var result = await query
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CommonQueueResponse
                {
                    Id = x.Id,
                    EnquiryId = x.EnquiryNumber,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    //VehicleNumber = x.VehicleEnquiryDetails!
                    //    .VehicleDetails!
                    //    .VehicleNumber,
                    //VehicleType = x.VehicleEnquiryDetails!
                    //    .VehicleType!
                    //    .TypeName,
                    InsuranceType = x.InsuranceType!
                        .Name,
                    //PolicyType = x.VehicleEnquiryDetails!
                    //    .MIPolicyType,
                    CreatedBy = x.CreatedByUser!
                        .FullName,
                    CreatedByPhone = x.CreatedByUser!
                        .PhoneNumber,
                    CreatedByRole = x.CreatedByUser!
                        .UserType,
                    AgencyName = x.Tenant!
                        .AgentName,
                    SubmittedOn = x.UpdatedAt,
                    Priority = x.Priority,
                    Status = x.Status
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            var pagedResult = new PagedResponse<CommonQueueResponse>
            {
                Items = result,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result<PagedResponse<CommonQueueResponse>>.Success(
                pagedResult,
                "Enquiries fetched successfully");
        }
        #endregion

        #region Admin - Enquiry - Quote Queue ( enquiries assigned to current logged user for quote or quoted )
        public async Task<Result<PagedResponse<QuoteQueueResponse>>> MyQuoteQueue(QuoteQueueFilter filter,CancellationToken ct)
        {
            var currentUserId = _currentUser.UserId;

            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking()
                .Where(x => x.AssignedEmployeeId == currentUserId);

            if (filter.Type == QuoteQueueType.Quote)
            {
                query = query.Where(x =>
                    (
                        x.Status == EnquiryStatus.Submitted.ToString() ||
                        x.Status == EnquiryStatus.QuoteInProgress.ToString()
                    ) &&
                    (
                        x.AdminAssisted == true ||
                        x.CurrentStep == 4
                    ));
            }
            else if (filter.Type == QuoteQueueType.Quoted)
            {
                query = query.Where(x =>
                    x.Status == EnquiryStatus.Quoted.ToString() &&
                    x.EnquiryQuotes.Any());
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.DateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.ThisYear:
                    fromDate = new DateOnly(
                        today.Year,
                        1,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                default:
                    return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                        "Invalid date filter.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);

            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.CreatedAt >= from &&
                x.CreatedAt < toExclusive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.CustomerName.Contains(search) ||
                    x.CustomerNumber.Contains(search) ||
                    (
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            .Contains(search)
                    ));
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                var insuranceTypeId = filter.InsuranceTypeId.Value;

                query = query.Where(x =>
                    x.InsuranceTypeId == insuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                var vehicleTypeId = filter.VehicleTypeId.Value;

                query = query.Where(x =>
                    x.VehicleEnquiryDetails != null &&
                    x.VehicleEnquiryDetails.VehicleTypeId == vehicleTypeId);
            }

            if (filter.AccountId.HasValue)
            {
                var accountId = filter.AccountId.Value;

                query = query.Where(x =>
                    x.TenantId == accountId);
            }

            var totalCount = await query.CountAsync(ct);

            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new QuoteQueueResponse
                {
                    Id = x.Id,
                    EnquiryId = x.EnquiryNumber,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    //VehicleNumber = x.VehicleEnquiryDetails!
                    //    .VehicleDetails!
                    //    .VehicleNumber,
                    //VehicleType = x.VehicleEnquiryDetails!
                    //    .VehicleType!
                    //    .TypeName,
                    InsuranceType = x.InsuranceType!
                        .Name,
                    //PolicyType = x.VehicleEnquiryDetails!
                    //    .MIPolicyType,
                    CreatedBy = x.CreatedByUser!
                        .FullName,
                    CreatedByPhone = x.CreatedByUser!
                        .PhoneNumber,
                    CreatedByRole = x.CreatedByUser!
                        .UserType,
                    AgencyName = x.Tenant!
                        .AgentName,
                    SubmittedOn = x.UpdatedAt,
                    Status = x.Status,
                    AdminAssisted = x.AdminAssisted,
                    //QuoteCount = x.EnquiryQuotes.Count(),
                    AssignedEmployee = x.AssignedEmployee != null
                        ? x.AssignedEmployee.FullName
                        : null,
                    AssignedEmployeeId = x.AssignedEmployeeId,
                    AssignedEmployeeRole = x.AssignedEmployee != null
                        ? x.AssignedEmployee.UserType
                        : null
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            var pagedResult = new PagedResponse<QuoteQueueResponse>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result<PagedResponse<QuoteQueueResponse>>.Success(
                pagedResult,
                "Enquiries fetched successfully");
        }
        #endregion

        #region Admin - Enquiry - Policy Queue ( enquiries assigned to current logged user for policy or policy issued )
        public async Task<Result<PagedResponse<QuoteQueueResponse>>> MyPolicyQueue(PolicyQueueFilter filter, CancellationToken ct)
        {
            var currentUserId = _currentUser.UserId;

            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking()
                .Where(x => x.AssignedEmployeeId == currentUserId);

            if (filter.Type == PolicyQueueType.Policy)
            {
                query = query.Where(x =>
                    x.Status == EnquiryStatus.PaymentVerificationPending.ToString() ||
                    x.Status == EnquiryStatus.PolicyInProgress.ToString());
            }
            else if (filter.Type == PolicyQueueType.IssuedPolicy)
            {
                query = query.Where(x =>
                    x.Status == EnquiryStatus.PolicyIssued.ToString());
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.DateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.ThisYear:
                    fromDate = new DateOnly(
                        today.Year,
                        1,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                default:
                    return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                        "Invalid date filter.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);

            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.CreatedAt >= from &&
                x.CreatedAt < toExclusive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.CustomerName.Contains(search) ||
                    x.CustomerNumber.Contains(search) ||
                    (
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            .Contains(search)
                    ));
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                var insuranceTypeId = filter.InsuranceTypeId.Value;

                query = query.Where(x =>
                    x.InsuranceTypeId == insuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                var vehicleTypeId = filter.VehicleTypeId.Value;

                query = query.Where(x =>
                    x.VehicleEnquiryDetails != null &&
                    x.VehicleEnquiryDetails.VehicleTypeId == vehicleTypeId);
            }

            if (filter.AccountId.HasValue)
            {
                var accountId = filter.AccountId.Value;

                query = query.Where(x =>
                    x.TenantId == accountId);
            }

            var totalCount = await query.CountAsync(ct);

            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new QuoteQueueResponse
                {
                    Id = x.Id,
                    EnquiryId = x.EnquiryNumber,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    //VehicleNumber = x.VehicleEnquiryDetails!
                    //    .VehicleDetails!
                    //    .VehicleNumber,
                    //VehicleType = x.VehicleEnquiryDetails!
                    //    .VehicleType!
                    //    .TypeName,
                    InsuranceType = x.InsuranceType!
                        .Name,
                    //PolicyType = x.VehicleEnquiryDetails!
                    //    .MIPolicyType,
                    CreatedBy = x.CreatedByUser!
                        .FullName,
                    CreatedByPhone = x.CreatedByUser!
                        .PhoneNumber,
                    CreatedByRole = x.CreatedByUser!
                        .UserType,
                    AgencyName = x.Tenant!
                        .AgentName,
                    SubmittedOn = x.UpdatedAt,
                    Status = x.Status,
                    CurrentStep = x.CurrentStep,
                    //AssignedEmployee = x.AssignedEmployee != null
                    //    ? x.AssignedEmployee.FullName
                    //    : null,
                    //AssignedEmployeeId = x.AssignedEmployeeId,
                    //AssignedEmployeeRole = x.AssignedEmployee != null
                    //    ? x.AssignedEmployee.UserType
                    //    : null
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            var pagedResult = new PagedResponse<QuoteQueueResponse>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result<PagedResponse<QuoteQueueResponse>>.Success(
                pagedResult,
                filter.Type == PolicyQueueType.Policy
                    ? "Policy enquiries fetched successfully."
                    : "Issued policy enquiries fetched successfully.");
        }
        #endregion

        #region Admin - Enquiry - Draft Queue
        public async Task<Result<PagedResponse<QuoteQueueResponse>>> DraftQueue(PolicyQueueFilter filter, CancellationToken ct)
        {
            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking()
                .Where(x =>
                    x.Status == EnquiryStatus.Created.ToString() &&
                    x.CurrentStep == 2);

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.DateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.ThisYear:
                    fromDate = new DateOnly(
                        today.Year,
                        1,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                default:
                    return Result<PagedResponse<QuoteQueueResponse>>.Failure(
                        "Invalid date filter.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);

            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.CreatedAt >= from &&
                x.CreatedAt < toExclusive);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                    x.CustomerName.Contains(search) ||
                    x.CustomerNumber.Contains(search) ||
                    (
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails != null &&
                        x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            .Contains(search)
                    ));
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                var insuranceTypeId = filter.InsuranceTypeId.Value;

                query = query.Where(x =>
                    x.InsuranceTypeId == insuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                var vehicleTypeId = filter.VehicleTypeId.Value;

                query = query.Where(x =>
                    x.VehicleEnquiryDetails != null &&
                    x.VehicleEnquiryDetails.VehicleTypeId == vehicleTypeId);
            }

            if (filter.AccountId.HasValue)
            {
                var accountId = filter.AccountId.Value;

                query = query.Where(x =>
                    x.TenantId == accountId);
            }

            var totalCount = await query.CountAsync(ct);

            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new QuoteQueueResponse
                {
                    Id = x.Id,
                    EnquiryId = x.EnquiryNumber,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    //VehicleNumber = x.VehicleEnquiryDetails!
                    //    .VehicleDetails!
                    //    .VehicleNumber,
                    //VehicleType = x.VehicleEnquiryDetails!
                    //    .VehicleType!
                    //    .TypeName,
                    InsuranceType = x.InsuranceType!
                        .Name,
                    //PolicyType = x.VehicleEnquiryDetails!
                    //    .MIPolicyType,
                    CreatedBy = x.CreatedByUser!
                        .FullName,
                    CreatedByPhone = x.CreatedByUser!
                        .PhoneNumber,
                    CreatedByRole = x.CreatedByUser!
                        .UserType,
                    AgencyName = x.Tenant!
                        .AgentName,
                    SubmittedOn = x.UpdatedAt,
                    Status = x.Status,
                    CurrentStep = x.CurrentStep
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            var pagedResult = new PagedResponse<QuoteQueueResponse>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Result<PagedResponse<QuoteQueueResponse>>.Success(
                pagedResult,
                "Draft enquiries fetched successfully.");
        }
        #endregion

        #region Admin - Enquiry - Dashboard
        public async Task<Result<AdminEnquiryDashboardResponse>> GetAdminEnquiryDashboardAsync(EnquiryDashboardFilter filter, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var query = _enquiryRepository
                .GetEnquiryQueryable()
                .AsNoTracking();

            var today = DateOnly.FromDateTime(DateTime.Today);

            DateOnly fromDate;
            DateOnly toDate;

            switch (filter.dateFilter)
            {
                case DateFilterType.Today:
                    fromDate = today;
                    toDate = today;
                    break;

                case DateFilterType.Yesterday:
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;

                case DateFilterType.ThisWeek:
                    var daysSinceMonday =
                        ((int)today.DayOfWeek + 6) % 7;

                    fromDate = today.AddDays(-daysSinceMonday);
                    toDate = today;
                    break;

                case DateFilterType.ThisMonth:
                    fromDate = new DateOnly(
                        today.Year,
                        today.Month,
                        1);

                    toDate = today;
                    break;

                case DateFilterType.Custom:
                    if (!filter.FromDate.HasValue ||
                        !filter.ToDate.HasValue)
                    {
                        return Result<AdminEnquiryDashboardResponse>.Failure(
                            "FromDate and ToDate are required for custom date range.");
                    }

                    if (filter.FromDate > filter.ToDate)
                    {
                        return Result<AdminEnquiryDashboardResponse>.Failure(
                            "FromDate cannot be greater than ToDate.");
                    }

                    fromDate = filter.FromDate.Value;
                    toDate = filter.ToDate.Value;
                    break;

                default:
                    return Result<AdminEnquiryDashboardResponse>.Failure(
                        "Invalid dashboard date range.");
            }

            var from = fromDate.ToDateTime(TimeOnly.MinValue);

            var toExclusive = toDate
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            query = query.Where(x =>
                x.CreatedAt >= from &&
                x.CreatedAt < toExclusive);

            if (filter.AccountId.HasValue)
            {
                query = query.Where(x =>
                    x.TenantId == filter.AccountId.Value);
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                query = query.Where(x =>
                    x.InsuranceTypeId == filter.InsuranceTypeId.Value);
            }

            var dashboard = await query
                .GroupBy(x => 1)
                .Select(g => new AdminEnquiryDashboardResponse
                {
                    TotalCount = g.Count(),

                    DraftCount = g.Count(x =>
                        x.Status == EnquiryStatus.Created.ToString() &&
                        x.CurrentStep == 2),

                    CommonQueueCount = g.Count(x =>
                        x.AssignedEmployeeId == null),

                    QuoteCount = g.Count(x =>
                        (
                            x.Status == EnquiryStatus.Submitted.ToString() ||
                            x.Status == EnquiryStatus.QuoteInProgress.ToString()
                        ) &&
                        x.AssignedEmployeeId == userId &&
                        (
                            x.AdminAssisted == true ||
                            x.CurrentStep == 4
                        )),

                    QuotedCount = g.Count(x =>
                        x.Status == EnquiryStatus.Quoted.ToString() && x.AssignedEmployeeId == userId),

                    PolicyCount = g.Count(x =>
                        x.Status == EnquiryStatus.PaymentVerificationPending.ToString() && x.AssignedEmployeeId == userId ||
                        (
                            x.Status == EnquiryStatus.PolicyInProgress.ToString()

                        )),

                    IssuedPolicyCount = g.Count(x =>
                        x.Status == EnquiryStatus.PolicyIssued.ToString() && x.AssignedEmployeeId == userId),

                    MyTaskCount = g.Count(x =>
                        x.AssignedEmployeeId == userId),

                    UnassignedCount = g.Count(x =>
                        x.AssignedEmployeeId == null),

                    AdminCreatedCount = g.Count(x =>
                        x.AdminAssisted == true)
                })
                .FirstOrDefaultAsync(ct);

            dashboard ??= new AdminEnquiryDashboardResponse();

            return Result<AdminEnquiryDashboardResponse>.Success(
                dashboard,
                "Enquiry dashboard fetched successfully.");
        }
        #endregion

        #region Admin Policy Summary
        public async Task<Result<PolicySummaryDto>> GetPolicySummaryAsync(Guid enquiryId,CancellationToken ct)
        {
            var summary = await _enquiryRepository
                .GetQueryable()
                .Where(x => x.Id == enquiryId)
                .Select(x => new PolicySummaryDto
                {
                    EnquiryId = x.Id,

                    // Customer
                    CustomerName = x.CustomerName,
                    Mobile = x.CustomerNumber,
                    Address = x.VehicleEnquiryDetails.VehicleDetails.PermanentAddress,

                    // Vehicle
                    VehicleNumber = x.VehicleEnquiryDetails!
                        .VehicleDetails!
                        .VehicleNumber,

                    Vehicle = x.VehicleEnquiryDetails!
                        .VehicleDetails!
                        .Model,

                    VehicleType = x.VehicleEnquiryDetails!
                        .VehicleType!
                        .TypeName,

                    FuelType = x.VehicleEnquiryDetails!
                        .VehicleDetails!
                        .FuelType,

                    // Insurance
                    InsuranceCompany = x.EnquiryQuotes
                        .Where(q => q.IsSelected)
                        .Select(q => q.InsuranceCompany.CompanyName)
                        .FirstOrDefault() ?? string.Empty,

                    PolicyType = x.VehicleEnquiryDetails!
                        .MIPolicyType,

                    // Financial
                    PremiumAmount = x.EnquiryQuotes
                        .Where(q => q.IsSelected)
                        .Select(q => q.Premium)
                        .FirstOrDefault(),

                    CompanyPayable = _currentUser.Role == "Admin" || _currentUser.Role == "Supervisor" || _currentUser.Role == "AdminEmployee" ? x.EnquiryQuotes
                        .Where(q => q.IsSelected)
                        .Select(q => q.CompanyPayable)
                        .FirstOrDefault() : null ,

                    AdminProfit = _currentUser.Role == "Admin" || _currentUser.Role == "Supervisor" || _currentUser.Role == "AdminEmployee" ? x.EnquiryQuotes
                        .Where(q => q.IsSelected)
                        .Select(q => q.AdminProfit)
                        .FirstOrDefault() : null,

                    AgentPayable = x.EnquiryQuotes
                        .Where(q => q.IsSelected)
                        .Select(q => q.Premium - q.AgentPoints)
                        .FirstOrDefault(),

                    AgentProfit = x.EnquiryQuotes
                        .Where(q => q.IsSelected)
                        .Select(q => q.AgentPoints)
                        .FirstOrDefault(),

                    SelectedQuoteId = x.EnquiryQuotes
                        .Where(q=> q.EnquiryId == enquiryId && q.IsSelected)
                        .Select(q => q.Id)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);

            if (summary == null)
            {
                return Result<PolicySummaryDto>.NotFound(
                    "Policy summary not found.");
            }

            return Result<PolicySummaryDto>.Success(
                summary,
                "Policy summary fetched successfully.");
        }
        #endregion

        #region Admin Payment Details Summary
        public async Task<Result<PaymentDetailsDto>> GetPaymentDetailsAsync(Guid enquiryId,CancellationToken ct)
        {
            var payment = await _policyPaymentRepository
                .GetQueryable()
                .Where(x => x.EnquiryId == enquiryId)
                .Select(x => new PaymentDetailsDto
                {
                    EnquiryId = x.EnquiryId,

                    PaymentMode = x.PaymentMode,
                    Amount = x.Amount,
                    Remarks = x.Remarks,
                    ReferenceNumber = x.ReferenceNumber,

                    // Ledger
                    AgentLedgerName = x.PaymentMode == "Ledger"
                        ? x.Account.AgentName
                        : null,

                    AgentTotalLimit = x.PaymentMode == "Ledger"
                        ? x.Account.Ledger.CreditLimit
                        : null,

                    CurrentBalance = x.PaymentMode == "Ledger"
                        ? x.Account.Ledger.AvailableBalance
                        : null,

                    TransactionId = x.PaymentMode == "Ledger"
                        ? null
                        : x.TransactionId,

                    LedgerPaymentTime = x.PaymentMode == "Ledger"
                        ? x.SubmittedAt
                        : null,

                    // Verification
                    VerifiedByName = x.VerifiedByUser != null
                        ? x.VerifiedByUser.FullName
                        : null,

                    VerifiedAt = x.VerifiedAt,

                    // Bank Transfer
                    AccountName = x.PaymentMode == "BankTransfer"
                        ? x.BankAccount!.AccountName
                        : null,

                    UpiId = x.PaymentMode == "BankTransfer"
                        ? x.BankAccount!.UpiId
                        : null,

                    AccountNumber = x.PaymentMode == "BankTransfer"
                        ? x.BankAccount!.AccountNumber
                        : null,

                    IFSC = x.PaymentMode == "BankTransfer"
                        ? x.BankAccount!.IFSCCode
                        : null,

                    PaymentDate = x.PaymentMode == "BankTransfer"
                        ? x.SubmittedAt
                        : null
                })
                .FirstOrDefaultAsync(ct);

            if (payment == null)
            {
                return Result<PaymentDetailsDto>.NotFound(
                    "Payment details not found.");
            }

            return Result<PaymentDetailsDto>.Success(
                payment,
                "Payment details fetched successfully.");
        }
        #endregion

        #region Admin Vendor Settlement Details
        public async Task<Result<VendorSettlementDetailsDto>> GetVendorSettlementDetailsAsync(Guid enquiryId,Guid vendorId,VendorSettlementMethod settlementMethod,CancellationToken ct)
        {
            var vendor = await _vendorRepository
                .GetQueryable()
                .Where(x => x.Id == vendorId && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .FirstOrDefaultAsync(ct);

            if (vendor == null)
            {
                return Result<VendorSettlementDetailsDto>.NotFound(
                    "Vendor not found.");
            }

            var quote = await _enquiryRepository
                .GetQueryable()
                .Where(x => x.Id == enquiryId)
                .Select(x => x.EnquiryQuotes
                    .Where(q => q.IsSelected)
                    .Select(q => new
                    {
                        AgentPaid = q.Premium - q.AgentPoints,
                        q.CompanyPayable
                    })
                    .FirstOrDefault())
                .FirstOrDefaultAsync(ct);

            if (quote == null)
            {
                return Result<VendorSettlementDetailsDto>.NotFound(
                    "Selected quote not found.");
            }

            var result = new VendorSettlementDetailsDto
            {
                VendorId = vendor.Id,
                VendorName = vendor.Name,
                SettlementMethod = settlementMethod.ToString(),
                CompanyPayable = quote.CompanyPayable
            };

            switch (settlementMethod)
            {
                case VendorSettlementMethod.Direct:

                    result.AgentPaid = quote.AgentPaid;

                    result.CommissionToReceive =
                        quote.AgentPaid - quote.CompanyPayable;

                    break;

                case VendorSettlementMethod.CommissionAdjustment:

                    var pendingCommission = await _vendorTransactionRepository
                        .GetQueryable()
                        .Where(x => x.VendorId == vendorId)
                        .SumAsync(x =>
                            x.TransactionType == VendorTransactionType.CommissionEarned.ToString()
                                ? x.Amount
                                : x.TransactionType == VendorTransactionType.CommissionReceived.ToString() ||
                                  x.TransactionType == VendorTransactionType.PolicyAdjustment.ToString()
                                    ? -x.Amount
                                    : 0,
                            ct);

                    pendingCommission = Math.Max(0, pendingCommission);

                    var adjustedAmount = Math.Min(
                        pendingCommission,
                        quote.CompanyPayable);

                    result.PendingCommission = pendingCommission;

                    result.CommissionAdjusted = adjustedAmount;

                    result.RemainingCommission =
                        pendingCommission - adjustedAmount;

                    break;

                case VendorSettlementMethod.AdminPayment:

                    // Only CompanyPayable is required.
                    break;
            }

            return Result<VendorSettlementDetailsDto>.Success(
                result,
                "Vendor settlement details fetched successfully.");
        }
        #endregion

        #region Admin Create Policy Details
        public async Task<Result<PolicyDetailsDto>> CreatePolicyDetailsAsync(Guid enquiryId,CreatePolicyDetailsRequest request,CancellationToken ct)
        {
            var enquiryExists = await _enquiryRepository
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == enquiryId, ct);

            if (enquiryExists == null)
            {
                return Result<PolicyDetailsDto>.NotFound(
                    "Enquiry not found.");
            }

            if (string.IsNullOrWhiteSpace(request.PolicyNumber))
            {
                return Result<PolicyDetailsDto>.Failure(
                    "Policy number is required.", 400);
            }

            if (request.PolicyEndDate <= request.PolicyStartDate)
            {
                return Result<PolicyDetailsDto>.Failure(
                    "Policy end date must be after policy start date.", 400);
            }

            var existingPolicy = await _policyDetailsRepository
                .GetQueryable()
                .FirstOrDefaultAsync(
                    x => x.EnquiryId == enquiryId,
                    ct);

            if (existingPolicy != null)
            {
                return Result<PolicyDetailsDto>.Failure(
                    "Policy details already exist for this enquiry.", 409);
            }

            var policyDetails = new PolicyDetails
            {
                EnquiryId = enquiryId,
                PolicyNumber = request.PolicyNumber.Trim(),
                PolicyStartDate = request.PolicyStartDate,
                PolicyEndDate = request.PolicyEndDate,
                Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                    ? null
                    : request.Remarks.Trim(),
                CreatedByUserIpAddress = _currentUser.IpAddress,
                CreatedByUserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _policyDetailsRepository.AddAsync(
                policyDetails,
                ct);

            enquiryExists.Status = EnquiryStatus.PolicyIssued.ToString();

            _enquiryRepository.Update(enquiryExists);

            await _unitOfWork.SaveChangesAsync();

            var result = new PolicyDetailsDto
            {
                Id = policyDetails.Id,
                EnquiryId = policyDetails.EnquiryId,
                PolicyNumber = policyDetails.PolicyNumber,
                PolicyStartDate = policyDetails.PolicyStartDate,
                PolicyEndDate = policyDetails.PolicyEndDate,
                Remarks = policyDetails.Remarks,
                CreatedAt = policyDetails.CreatedAt
            };

            return Result<PolicyDetailsDto>.Success(
                result,
                "Policy details created successfully.");
        }

        public async Task<Result<PolicyDetailsDto>> GetPolicyDetailsAsync(Guid enquiryId,CancellationToken ct)
        {
            var policy = await _policyDetailsRepository
                .GetQueryable()
                .Where(x => x.EnquiryId == enquiryId)
                .Select(x => new PolicyDetailsDto
                {
                    Id = x.Id,
                    EnquiryId = x.EnquiryId,
                    PolicyNumber = x.PolicyNumber,
                    PolicyStartDate = x.PolicyStartDate,
                    PolicyEndDate = x.PolicyEndDate,
                    Remarks = x.Remarks,
                    CreatedBy = x.CreatedByUser.FullName,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync(ct);

            if (policy == null)
            {
                return Result<PolicyDetailsDto>.NotFound(
                    "Policy details not found.");
            }

            return Result<PolicyDetailsDto>.Success(
                policy,
                "Policy details fetched successfully.");
        }
        #endregion

        #region Submit Vendor Settlement
        public async Task<Result<bool>> SubmitVendorSettlementAsync(SubmitVendorSettlementRequest request,CancellationToken ct)
        {
            var vendor = await _vendorRepository
                .GetQueryable()
                .FirstOrDefaultAsync(
                    x => x.Id == request.VendorId && x.IsActive,
                    ct);

            if (vendor == null)
            {
                return Result<bool>.NotFound("Vendor not found.");
            }

            var quote = await _enquiryRepository
                .GetQueryable()
                .Where(x => x.Id == request.EnquiryId)
                .Select(x => x.EnquiryQuotes
                    .Where(q => q.IsSelected)
                    .Select(q => new
                    {
                        AgentPaid = q.Premium - q.AgentPoints,
                        q.CompanyPayable
                    })
                    .FirstOrDefault())
                .FirstOrDefaultAsync(ct);

            if (quote == null)
            {
                return Result<bool>.NotFound(
                    "Selected quote not found.");
            }

            switch (request.SettlementMethod)
            {
                case VendorSettlementMethod.Direct:
                    {
                        var commissionToReceive =
                            quote.AgentPaid - quote.CompanyPayable;

                        if (commissionToReceive < 0)
                        {
                            return Result<bool>.Failure(
                                "Commission amount cannot be negative.", 400);
                        }

                        var transaction = new VendorTransaction
                        {
                            VendorId = request.VendorId,
                            EnquiryId = request.EnquiryId,

                            TransactionType =
                                VendorTransactionType.CommissionEarned.ToString(),

                            Amount = commissionToReceive,

                            ReferenceNumber = request.ReferenceNumber,
                            PaymentMode = "Bank Transfer",
                            Remarks = request.Remarks,

                            TransactionDate = DateTime.UtcNow,

                            CreatedByUserId = _currentUser.UserId,

                            BalanceAfter = 0
                        };

                        // Get previous vendor balance
                        var previousBalance = await GetVendorBalance(
                            request.VendorId,
                            ct);

                        transaction.BalanceAfter =
                            previousBalance + commissionToReceive;

                        await _vendorTransactionRepository.AddAsync(
                            transaction,
                            ct);

                        break;
                    }

                case VendorSettlementMethod.CommissionAdjustment:
                    {
                        var pendingCommission =
                            await GetVendorBalance(
                                request.VendorId,
                                ct);

                        if (pendingCommission <= 0)
                        {
                            return Result<bool>.Failure(
                                "Vendor has no pending commission.", 400);
                        }

                        var adjustedAmount = Math.Min(
                            pendingCommission,
                            quote.CompanyPayable);

                        var remainingBalance =
                            pendingCommission - adjustedAmount;

                        var transaction = new VendorTransaction
                        {
                            VendorId = request.VendorId,
                            EnquiryId = request.EnquiryId,

                            TransactionType =
                                VendorTransactionType.PolicyAdjustment.ToString(),

                            Amount = adjustedAmount,

                            BalanceAfter = remainingBalance,

                            ReferenceNumber = request.ReferenceNumber,
                            PaymentMode = "CommissionAdjustment",
                            Remarks = request.Remarks,

                            TransactionDate = DateTime.UtcNow,

                            CreatedByUserId = _currentUser.UserId
                        };

                        await _vendorTransactionRepository.AddAsync(
                            transaction,
                            ct);

                        break;
                    }

                    case VendorSettlementMethod.AdminPayment:
                    {
                        if (!request.BankAccountId.HasValue)
                        {
                            return Result<bool>.Failure(
                                "Bank account is required for admin payment.",
                                400);
                        }

                        var bankAccount = await _bankAccountRepository
                            .GetQueryable()
                            .FirstOrDefaultAsync(
                                x => x.Id == request.BankAccountId.Value &&
                                     x.IsActive,
                                ct);

                        if (bankAccount == null)
                        {
                            return Result<bool>.NotFound(
                                "Bank account not found.");
                        }

                        var previousBalance = await GetVendorBalance(
                            request.VendorId,
                            ct);

                        var previousBankBalance = bankAccount.OpeningBalance;

                        // Vendor transaction
                        var vendorTransaction = new VendorTransaction
                        {
                            VendorId = request.VendorId,
                            EnquiryId = request.EnquiryId,

                            TransactionType =
                                VendorTransactionType.VendorPayment.ToString(),

                            Amount = quote.CompanyPayable,

                            // Commission balance is NOT affected
                            BalanceAfter = previousBalance,

                            ReferenceNumber = request.ReferenceNumber,
                            PaymentMode = "Bank Transfer",
                            Remarks = request.Remarks,

                            TransactionDate = DateTime.UtcNow,
                            CreatedByUserId = _currentUser.UserId
                        };

                        await _vendorTransactionRepository.AddAsync(
                            vendorTransaction,
                            ct);


                        // Bank transaction
                        var bankTransaction = new BankTransaction
                        {
                            BankAccountId = bankAccount.Id,

                            TransactionType = "Debit",
                            Amount = quote.CompanyPayable,

                            Category = "VendorPayment",

                            Description =
                                $"Payment to vendor for enquiry {request.EnquiryId}",

                            VendorId = request.VendorId,
                            

                            TransactionDate = DateTime.UtcNow,

                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _bankTransactionRepository.AddAsync(
                            bankTransaction,ct);

                        bankAccount.CurrentBalance -= quote.CompanyPayable;
                        bankAccount.UpdatedAt = DateTime.UtcNow;

                        _bankAccountRepository.Update(bankAccount);

                        break;
                    }

                default:
                    return Result<bool>.Failure(
                        "Invalid settlement method.", 400);
            }

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(
                true,
                "Vendor settlement submitted successfully.");
        }
        #endregion

        #region Helper Vendor Balance
        private async Task<decimal> GetVendorBalance(Guid vendorId,CancellationToken ct)
        {
            return await _vendorTransactionRepository
                .GetQueryable()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.TransactionDate)
                .Select(x => (decimal?)x.BalanceAfter)
                .FirstOrDefaultAsync(ct) ?? 0;
        }
        #endregion

        #region Settlement Completion Check
        public async Task<Result<bool>> IsVendorSettlementCompletedAsync(Guid enquiryId,CancellationToken ct)
        {
            var isSettled = await _vendorTransactionRepository
                .GetQueryable()
                .AnyAsync(x =>
                    x.EnquiryId == enquiryId &&
                    (
                        x.TransactionType == VendorTransactionType.VendorPayment.ToString() ||
                        x.TransactionType == VendorTransactionType.PolicyAdjustment.ToString() ||
                        x.TransactionType == VendorTransactionType.CommissionEarned.ToString()
                    ),
                    ct);

            return Result<bool>.Success(
                isSettled,
                "Vendor settlement status fetched successfully.");
        }
        #endregion

        #region Admin Change Enquiry Status

        public async Task<Result<bool>> ChangeEnquiryStatusAsync(ChangeEnquiryStatusRequest request,CancellationToken ct)
        {
            var enquiry = await _enquiryRepository
                .GetEnquiryQueryable()
                .FirstOrDefaultAsync(
                    x => x.Id == request.EnquiryId,
                    ct);

            if (enquiry == null)
            {
                return Result<bool>.NotFound(
                    "Enquiry not found.");
            }

            // Verify current status
            if (!string.Equals(
                    enquiry.Status,
                    request.CurrentStatus,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Result<bool>.Failure(
                    $"Enquiry is currently in '{enquiry.Status}' status. " +
                    $"It cannot be changed from '{request.CurrentStatus}'.",
                    400);
            }

            // QuoteInProgress -> Quoted
            if (string.Equals(
                    request.CurrentStatus,
                    EnquiryStatus.QuoteInProgress.ToString(),
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(
                    request.NewStatus,
                    EnquiryStatus.Quoted.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                var hasQuotes = await _enquiryQuoteRepository
                    .GetEnquiryQuoteQuery()
                    .AnyAsync(
                        x => x.EnquiryId == request.EnquiryId,
                        ct);

                if (!hasQuotes)
                {
                    return Result<bool>.Failure(
                        "Cannot change status to Quoted. No quotes are available.",
                        400);
                }
            }

            // PolicyInProgress -> PolicyIssued
            if (string.Equals(
                    request.CurrentStatus,
                    EnquiryStatus.PolicyInProgress.ToString(),
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(
                    request.NewStatus,
                    EnquiryStatus.PolicyIssued.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                // Check approved payment
                var hasApprovedPayment = await _policyPaymentRepository
                    .GetPolicyPaymentQueryable()
                    .AnyAsync(
                        x =>
                            x.EnquiryId == request.EnquiryId &&
                            x.Status == "Approved",
                        ct);

                if (!hasApprovedPayment)
                {
                    return Result<bool>.Failure(
                        "Cannot issue policy. Approved payment is not available.",
                        400);
                }

                // Check policy document
                var hasPolicyDocument = await _documentRepository
                    .GetDocuments()
                    .AnyAsync(
                        x =>
                            x.EnquiryId == request.EnquiryId &&
                            x.DocType == "Policy" &&
                            x.IsActive,
                        ct);

                if (!hasPolicyDocument)
                {
                    return Result<bool>.Failure(
                        "Cannot issue policy. Policy document has not been uploaded.",
                        400);
                }
            }

            // Update status
            enquiry.Status = request.NewStatus;
            enquiry.UpdatedAt = DateTime.UtcNow;

            _enquiryRepository.Update(enquiry);

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(
                true,
                "Enquiry status updated successfully.");
        }
        #endregion

        #region Agent Enquiry Lookup
        public async Task<Result<AgentEnquiryDashboardResponse>> GetAgentEnquiriesAsync(AgentEnquiryFilterDto filter,CancellationToken ct = default)
        {
            try
            {
                var tenantId = _currentUser.AccountId;
                var currentUserId = _currentUser.UserId;

                // UserType "Agent" represents the Agency Owner
                var isOwner = _currentUser.Role == "Agent";

                // Base query strictly scoped to tenant
                var baseQuery = _enquiryRepository
                    .GetEnquiryQueryable()
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId);

                // Scope enforcement: Owner has global agency access; staff are scoped to their assignments
                if (isOwner)
                {
                    if (filter.AgentId.HasValue)
                    {
                        baseQuery = baseQuery.Where(x => x.AssignedEmployeeId == filter.AgentId.Value);
                    }
                }
                else
                {
                    baseQuery = baseQuery.Where(x => x.AssignedEmployeeId == currentUserId);
                }

                // Apply Date Range filters on the base query so metrics match the selected date window
                var today = DateTime.UtcNow.Date;

                switch (filter.DateFilter)
                {
                    case DateFilterType.Today:
                        var todayEnd = today.AddDays(1);
                        baseQuery = baseQuery.Where(x => x.CreatedAt >= today && x.CreatedAt < todayEnd);
                        break;

                    case DateFilterType.Yesterday:
                        var yestStart = today.AddDays(-1);
                        baseQuery = baseQuery.Where(x => x.CreatedAt >= yestStart && x.CreatedAt < today);
                        break;

                    case DateFilterType.ThisWeek:
                        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                        var weekStart = today.AddDays(-1 * diff);
                        var weekEnd = weekStart.AddDays(7);
                        baseQuery = baseQuery.Where(x => x.CreatedAt >= weekStart && x.CreatedAt < weekEnd);
                        break;

                    case DateFilterType.ThisMonth:
                        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var monthEnd = monthStart.AddMonths(1);
                        baseQuery = baseQuery.Where(x => x.CreatedAt >= monthStart && x.CreatedAt < monthEnd);
                        break;

                    case DateFilterType.ThisYear:
                        var yearStart = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var yearEnd = yearStart.AddYears(1);
                        baseQuery = baseQuery.Where(x => x.CreatedAt >= yearStart && x.CreatedAt < yearEnd);
                        break;

                    case DateFilterType.Custom:
                        if (filter.FromDate.HasValue)
                        {
                            var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                            baseQuery = baseQuery.Where(x => x.CreatedAt >= from);
                        }

                        if (filter.ToDate.HasValue)
                        {
                            var to = filter.ToDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
                            baseQuery = baseQuery.Where(x => x.CreatedAt < to);
                        }
                        break;
                }

                // 1. Calculate the 6 metrics in a single database aggregation trip
                var metrics = await baseQuery
                    .GroupBy(_ => 1)
                    .Select(g => new EnquiryMetricsDto(
                        g.Count(),
                        g.Count(x => x.Status == "Created"),
                        g.Count(x => x.Status == "Submitted"),
                        g.Count(x => x.Status == "Quoted"),
                        g.Count(x => x.Status == "Issued" || x.Status == "PolicyIssued"),

                        g.Sum(x =>
                            ((x.Status == "Issued" || x.Status == "PolicyIssued")
                             && x.PolicyPayments.Any()) // പെയ്‌മെന്റ് ടേബിളിൽ റെക്കോർഡ് ഉണ്ടെങ്കിൽ
                                ? (x.EnquiryQuotes
                                    .Where(q => q.IsSelected) // സെലക്ട് ചെയ്ത ക്വോട്ട് ആണെങ്കിൽ (ഫീൽഡ് അനുസരിച്ച് മാറ്റുക)
                                    .Select(q => (decimal?)q.AgentPoints)
                                    .FirstOrDefault() ?? 0m)
                                : 0m
                        )
                    ))
                    .FirstOrDefaultAsync(ct) ?? new EnquiryMetricsDto(0, 0, 0, 0, 0, 0m);

                // 2. Build Table Query with keyword and attribute filters
                var tableQuery = baseQuery;

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.Trim();
                    tableQuery = tableQuery.Where(x =>
                        x.CustomerName.Contains(search) ||
                        x.CustomerNumber.Contains(search) ||
                        x.EnquiryNumber.Contains(search) ||
                        (x.VehicleEnquiryDetails != null &&
                         x.VehicleEnquiryDetails.VehicleDetails != null &&
                         x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber.Contains(search)));
                }

                if (filter.InsuranceTypeId.HasValue)
                {
                    tableQuery = tableQuery.Where(x => x.InsuranceTypeId == filter.InsuranceTypeId.Value);
                }

                if (filter.VehicleTypeId.HasValue)
                {
                    tableQuery = tableQuery.Where(x =>
                        x.VehicleEnquiryDetails != null &&
                        x.VehicleEnquiryDetails.VehicleTypeId == filter.VehicleTypeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.EnquiryStatus))
                {
                    tableQuery = tableQuery.Where(x => x.Status == filter.EnquiryStatus);
                }

                var totalCount = await tableQuery.CountAsync(ct);

                // 3. Sorting & Pagination
                var isAsc = string.Equals(filter.SortDirection, "Asc", StringComparison.OrdinalIgnoreCase);
                tableQuery = isAsc ? tableQuery.OrderBy(x => x.CreatedAt) : tableQuery.OrderByDescending(x => x.CreatedAt);

                var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
                var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

                // 4. Project into DTO with null safety across relations
                var enquiries = await tableQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new AgentEnquiryListDto(
                        x.Id,
                        x.EnquiryNumber,                                           // 1. Guid EnquiryId
                        x.CustomerName,                                            // 2. string? CustomerName
                        x.CustomerNumber,                                          // 3. string? PhoneNumber
                        x.VehicleEnquiryDetails != null && x.VehicleEnquiryDetails.VehicleDetails != null
                            ? x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber
                            : null,                                                // 4. string? VehicleNumber
                        x.InsuranceTypeId,                                         // 5. Guid? InsuranceTypeId
                        x.InsuranceType != null ? x.InsuranceType.Name : null,      // 6. string? InsuranceType
                        x.VehicleEnquiryDetails != null
                            ? x.VehicleEnquiryDetails.MIPolicyType
                            : null,                                                // 7. string? PolicyType
                        x.VehicleEnquiryDetails != null
                            ? x.VehicleEnquiryDetails.VehicleTypeId
                            : null,                                                // 8. Guid? VehicleTypeId
                        x.VehicleEnquiryDetails != null && x.VehicleEnquiryDetails.VehicleType != null
                            ? x.VehicleEnquiryDetails.VehicleType.TypeName
                            : null,                                                // 9. string? VehicleType
                        x.Status,                                                  // 10. string? Status
                        x.CreatedByUserId,                                                 // 11. Guid? AgentId
                        x.CreatedByUser != null ? x.CreatedByUser.FullName : null,                 // 12. string? AgentName
                        x.AssignedEmployeeId,                                      // 13. Guid? AssignedAgentId
                        x.AssignedEmployee != null
                            ? x.AssignedEmployee.FullName
                            : "Unassigned",                                        // 14. string? AssignedAgentName
                        x.CreatedAt                                                // 15. DateTime CreatedAt
                    ))
                    .ToListAsync(ct);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new AgentEnquiryDashboardResponse
                {
                    Metrics = metrics,
                    Enquiries = new PagedResponse<AgentEnquiryListDto>
                    {
                        Items = enquiries,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalRecords = totalCount,
                        TotalPages = totalPages
                    }
                };

                return Result<AgentEnquiryDashboardResponse>.Success(
                    response,
                    "Enquiries and metrics fetched successfully.");
            }
            catch (Exception ex)
            {
                return Result<AgentEnquiryDashboardResponse>.Failure(
                    "Unable to fetch enquiries.");
            }
        }
        #endregion

        #region Agent View - Policy
        public async Task<Result<AgentPolicyPreview>> GetAgentPolicyPreview(Guid EnquiryId,CancellationToken ct)
        {
            var policyPreview = await _policyDetailsRepository
                .GetQueryable()
                .Where(x => x.EnquiryId == EnquiryId)
                .Select(x => new AgentPolicyPreview(
                    x.Enquiry.EnquiryNumber,
                    x.Enquiry.EnquiryQuotes
                        .Where(q => q.EnquiryId == EnquiryId && q.IsSelected)
                        .Select(q => q.InsuranceCompany.CompanyName)
                        .FirstOrDefault(),
                    x.PolicyNumber,
                    x.PolicyStartDate,
                    x.PolicyEndDate,
                    x.Enquiry.Documents
                        .Where(d => d.EnquiryId == EnquiryId && d.DocType == "Policy" && d.IsActive)
                        .Select(d => d.Url)
                        .FirstOrDefault(),
                    x.CreatedByUser != null
                        ? x.CreatedByUser.FullName
                        : null,
                    x.CreatedByUser.PhoneNumber != null
                        ? x.CreatedByUser.PhoneNumber
                        : null
                ))
                .FirstOrDefaultAsync(ct);
            if (policyPreview == null)
            {
                return Result<AgentPolicyPreview>.NotFound(
                    "Policy details not found for the given enquiry.");
            }
            return Result<AgentPolicyPreview>.Success(
                policyPreview,
                "Policy preview fetched successfully.");
        }
        #endregion
    }
}

