using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Enquiry
{
    public class EnquiryLogicService: IEnquiryLogicService
    {
        #region DI
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<EnquiryLogicService> _logger;
        private readonly IUserTaskAllocationRepository _userTaskAllocationRepository;
        #endregion

        #region Constructor
        public EnquiryLogicService(
            IEnquiryRepository enquiryRepository,
            ICurrentUserService currentUser,
            ILogger<EnquiryLogicService> logger,
            IUserTaskAllocationRepository userTaskAllocationRepository)
        {
            _enquiryRepository = enquiryRepository;
            _currentUser = currentUser;
            _logger = logger;
            _userTaskAllocationRepository = userTaskAllocationRepository;
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
                        Quoted = g.Count(x => x.Status == EnquiryStatus.Quoted.ToString()),
                        PolicyIssued = g.Count(x => x.Status == EnquiryStatus.PolicyIssued.ToString())
                    })
                    .FirstOrDefaultAsync() ?? new EnquiryDashboardResponse();

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
                        Quoted = g.Count(x => x.Status == EnquiryStatus.Quoted.ToString()),
                        PolicyIssued = g.Count(x => x.Status == EnquiryStatus.PolicyIssued.ToString())
                    })
                    .FirstOrDefaultAsync() ?? new EnquiryDashboardResponse();

                return Result<EnquiryDashboardResponse>.Success(response, "Dashboard count fetched successfully");
            }
            catch(Exception ex)
            {
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

                if (filter.AgentId.HasValue)
                {
                    query = query.Where(x =>
                        x.TenantId == filter.AgentId);
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

                    CreatedByName = x.CreatedByUserId != null? 
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
            catch(Exception ex)
            {
                return Result<PagedResponse<EnquiryListResponse>>
                    .Failure("An error occurred while fetching enquiries",500);
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
            catch(Exception ex)
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
                        Remarks = q.Remarks
                    })
                    .ToList()
            };

            return Result<EnquiryDetailsResponse>.Success(response);
        }
        #endregion

        public async Task<Result<PagedResponse<CommonQueueResponse>>> GetCommonQueueAsync(CommonQueueFilter filter)
        {
            var query = _enquiryRepository.GetCommonQueue();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x =>
                    x.CustomerName.Contains(filter.Search) ||
                    x.CustomerNumber.Contains(filter.Search) ||
                    x.VehicleEnquiryDetails!.VehicleDetails!.VehicleNumber.Contains(filter.Search));
            }

            if (filter.InsuranceTypeId.HasValue)
            {
                query = query.Where(x => x.InsuranceTypeId == filter.InsuranceTypeId);
            }

            if (filter.VehicleTypeId.HasValue)
            {
                query = query.Where(x =>
                    x.VehicleEnquiryDetails!.VehicleTypeId == filter.VehicleTypeId);
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

            query = filter.SortDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new CommonQueueResponse
                {
                    EnquiryId = x.Id,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    VehicleNumber = x.VehicleEnquiryDetails!.VehicleDetails!.VehicleNumber,
                    VehicleType = x.VehicleEnquiryDetails.VehicleType.TypeName,
                    InsuranceType = x.InsuranceType.Name,
                    PolicyType = x.VehicleEnquiryDetails.MIPolicyType,
                    Status = x.AssignedEmployeeId == null ? "Unassigned" : "Assigned",
                    CreatedBy = x.CreatedByUser!.FullName,
                    CreatedByPhone = x.CreatedByUser!.PhoneNumber,
                    CreatedByRole = x.CreatedByUser!.UserType,
                    AgencyName = x.Tenant.AgentName,
                    SubmittedOn = x.UpdatedAt,
                    Priority = x.Priority
                })
                .ToListAsync();

            var pagedResult = new PagedResponse<CommonQueueResponse>
            {
                Items = items,
                TotalRecords = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return Result<PagedResponse<CommonQueueResponse>>.Success(
                pagedResult,
                "Enquiries fetched successfully");
        }

        public async Task<Result<PagedResponse<CommonQueueResponse>>> GetMyCommonQueueAsync(CommonQueueFilter filter)
        {
            var userId = _currentUser.UserId;

            var allocations = _userTaskAllocationRepository
                .GetQueryable()
                .Where(x => x.UserId == userId && x.CanAssign);

            var query = _enquiryRepository
                .GetQueryable()
                .Where(e =>
                    (e.Status == EnquiryStatus.Submitted.ToString() ||
                     e.Status == EnquiryStatus.StartQuote.ToString()) &&
                    e.AssignedEmployeeId == null &&
                    allocations.Any(a =>
                        a.InsuranceTypeId == e.InsuranceTypeId &&
                        (a.VehicleTypeId == null ||
                         a.VehicleTypeId == e.VehicleEnquiryDetails!.VehicleTypeId)));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(e =>
                    e.CustomerName.Contains(filter.Search) ||
                    e.CustomerNumber.Contains(filter.Search) ||
                    (e.VehicleEnquiryDetails != null &&
                     e.VehicleEnquiryDetails.VehicleDetails != null &&
                     e.VehicleEnquiryDetails.VehicleDetails.VehicleNumber.Contains(filter.Search)));
            }

            if (filter.InsuranceTypeId.HasValue)
                query = query.Where(e => e.InsuranceTypeId == filter.InsuranceTypeId);

            if (filter.VehicleTypeId.HasValue)
                query = query.Where(e =>
                    e.VehicleEnquiryDetails != null &&
                    e.VehicleEnquiryDetails.VehicleTypeId == filter.VehicleTypeId);

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(e => e.CreatedAt >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(e => e.CreatedAt <= to);
            }

            query = filter.SortDescending
                ? query.OrderByDescending(e => e.CreatedAt)
                : query.OrderBy(e => e.CreatedAt);

            var pagedResult = await query
                .Select(e => new CommonQueueResponse
                {
                    EnquiryId = e.Id,
                    CustomerName = e.CustomerName,
                    CustomerNumber = e.CustomerNumber,
                    VehicleNumber = e.VehicleEnquiryDetails!.VehicleDetails!.VehicleNumber,
                    VehicleType = e.VehicleEnquiryDetails.VehicleType.TypeName,
                    InsuranceType = e.InsuranceType.Name,
                    PolicyType = e.VehicleEnquiryDetails.MIPolicyType,
                    Status = "Unassigned",
                    CreatedBy = e.CreatedByUser!.FullName,
                    CreatedByPhone = e.CreatedByUser.PhoneNumber,
                    CreatedByRole = e.CreatedByUser.UserType,
                    AgencyName = e.Tenant.AgentName,
                    SubmittedOn = e.UpdatedAt,
                    Priority = e.Priority
                })
                .ToPagedResponseAsync(filter.PageNumber, filter.PageSize);

            return Result<PagedResponse<CommonQueueResponse>>.Success(
                pagedResult,
                "Common queue fetched successfully.");
        }
    }
}
