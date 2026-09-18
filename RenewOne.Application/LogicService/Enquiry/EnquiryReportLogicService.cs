using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Reports;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Enquiry
{
    public sealed class EnquiryReportLogicService : IEnquiryReportLogicService
    {
        private readonly IEnquiryReportRepository _repository;

        public EnquiryReportLogicService(
            IEnquiryReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<EnquiryReportResult> GenerateAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.ReportType switch
            {
                EnquiryReportType.Status =>
                    await GenerateStatusReportAsync(
                        request,
                        cancellationToken),

                EnquiryReportType.Cancelled =>
                    await GenerateCancelledReportAsync(
                        request,
                        cancellationToken),

                _ => throw new NotSupportedException(
                    $"Report type '{request.ReportType}' is not implemented yet.")
            };
        }

        #region Status Report

        private async Task<EnquiryReportResult> GenerateStatusReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = _repository.GetEnquiries();

            var filter = request.Filter;

            // Date filter
            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;

                query = query.Where(x =>
                    x.CreatedAt >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDateExclusive =
                    filter.ToDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.CreatedAt < toDateExclusive);
            }

            // Status filter
            if (filter.Statuses is { Count: > 0 })
            {
                query = query.Where(x =>
                    filter.Statuses.Contains(x.Status));
            }

            // Employee filter
            if (filter.EmployeeIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.AssignedEmployeeId.HasValue &&
                    filter.EmployeeIds.Contains(
                        x.AssignedEmployeeId.Value));
            }

            // Agency filter
            if (filter.AgencyIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    filter.AgencyIds.Contains(x.TenantId));
            }

            // Insurance type filter
            if (filter.InsuranceTypeIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    filter.InsuranceTypeIds.Contains(
                        x.InsuranceTypeId));
            }

            // Total enquiries
            var totalCount = await query
                .CountAsync(cancellationToken);

            // Total premium from selected quotes
            var totalPremium = await query
                .SelectMany(x => x.EnquiryQuotes
                    .Where(q => q.IsSelected)
                    .Select(q => q.Premium))
                .SumAsync(cancellationToken);

            // Total profit only for PolicyIssued enquiries
            var totalProfit = await query
                .Where(x => x.Status == "PolicyIssued")
                .SelectMany(x => x.EnquiryQuotes
                    .Where(q => q.IsSelected)
                    .Select(q => q.AdminProfit))
                .SumAsync(cancellationToken);

            // Pagination
            var totalPages = (int)Math.Ceiling(
                totalCount / (double)request.PageSize);

            // Fetch paginated rows
            var reportRows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new EnquiryStatusReportRow
                {
                    EnquiryNumber = x.Id,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,

                    Employee = x.AssignedEmployee != null
                        ? x.AssignedEmployee.FullName
                        : null,

                    Agency = x.Tenant != null
                        ? x.Tenant.AgentName
                        : null,

                    InsuranceType = x.InsuranceType != null
                        ? x.InsuranceType.Name
                        : null
                })
                .ToListAsync(cancellationToken);

            // Convert to generic report rows
            var rows = reportRows
                .Select(x => (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["enquiryNumber"] = x.EnquiryNumber,
                        ["customerName"] = x.CustomerName,
                        ["customerNumber"] = x.CustomerNumber,
                        ["status"] = x.Status,
                        ["createdAt"] = x.CreatedAt,
                        ["employee"] = x.Employee,
                        ["agency"] = x.Agency,
                        ["insuranceType"] = x.InsuranceType
                    })
                .ToList();

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.Status,

                Columns =
                [
                    new()
                    {
                        Key = "enquiryNumber",
                        Label = "Enquiry No"
                    },
                    new()
                    {
                        Key = "customerName",
                        Label = "Customer"
                    },
                    new()
                    {
                        Key = "customerNumber",
                        Label = "Mobile"
                    },
                    new()
                    {
                        Key = "status",
                        Label = "Status"
                    },
                    new()
                    {
                        Key = "createdAt",
                        Label = "Created Date"
                    },
                    new()
                    {
                        Key = "employee",
                        Label = "Employee"
                    },
                    new()
                    {
                        Key = "agency",
                        Label = "Agency"
                    },
                    new()
                    {
                        Key = "insuranceType",
                        Label = "Insurance Type"
                    }
                ],

                Metrics =
                [
                    new()
                    {
                        Key = "totalEnquiries",
                        Label = "Total Enquiries",
                        Value = totalCount,
                        Format = "number"
                    },
                    new()
                    {
                        Key = "totalPremium",
                        Label = "Total Premium",
                        Value = totalPremium,
                        Format = "currency"
                    },
                    new()
                    {
                        Key = "totalProfit",
                        Label = "Total Profit",
                        Value = totalProfit,
                        Format = "currency"
                    }
                ],

                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region Cancelled Report

        private async Task<EnquiryReportResult> GenerateCancelledReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = _repository.GetEnquiries();

            var filter = request.Filter;

            // Only cancelled enquiries
            query = query.Where(x =>
                x.Status == "Cancelled");

            // Cancellation date filter
            if (filter.CancellationFromDate.HasValue)
            {
                var fromDate =
                    filter.CancellationFromDate.Value.Date;

                query = query.Where(x =>
                    x.CancellationDate.HasValue &&
                    x.CancellationDate.Value >= fromDate);
            }

            if (filter.CancellationToDate.HasValue)
            {
                var toDateExclusive =
                    filter.CancellationToDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.CancellationDate.HasValue &&
                    x.CancellationDate.Value < toDateExclusive);
            }

            // Employee filter
            if (filter.EmployeeIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.AssignedEmployeeId.HasValue &&
                    filter.EmployeeIds.Contains(
                        x.AssignedEmployeeId.Value));
            }

            // Agency filter
            if (filter.AgencyIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    filter.AgencyIds.Contains(x.TenantId));
            }

            // Insurance type filter
            if (filter.InsuranceTypeIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    filter.InsuranceTypeIds.Contains(
                        x.InsuranceTypeId));
            }

            // Total cancelled
            var totalCount = await query
                .CountAsync(cancellationToken);

            // Pagination
            var totalPages = (int)Math.Ceiling(
                totalCount / (double)request.PageSize);

            // Fetch rows
            var reportRows = await query
                .OrderByDescending(x => x.CancellationDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new EnquiryCancelledReportRow
                {
                    EnquiryNumber = x.Id,
                    CustomerName = x.CustomerName,
                    CustomerNumber = x.CustomerNumber,
                    CancellationDate = x.CancellationDate,
                    CancellationReason = x.CancellationReason,

                    Employee = x.AssignedEmployee != null
                        ? x.AssignedEmployee.FullName
                        : null,

                    Agency = x.Tenant != null
                        ? x.Tenant.AgentName
                        : null,

                    InsuranceType = x.InsuranceType != null
                        ? x.InsuranceType.Name
                        : null
                })
                .ToListAsync(cancellationToken);

            // Convert rows
            var rows = reportRows
                .Select(x => (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["enquiryNumber"] = x.EnquiryNumber,
                        ["customerName"] = x.CustomerName,
                        ["customerNumber"] = x.CustomerNumber,
                        ["cancellationDate"] = x.CancellationDate,
                        ["cancellationReason"] = x.CancellationReason,
                        ["employee"] = x.Employee,
                        ["agency"] = x.Agency,
                        ["insuranceType"] = x.InsuranceType
                    })
                .ToList();

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.Cancelled,

                Columns =
                [
                    new()
                    {
                        Key = "enquiryNumber",
                        Label = "Enquiry No"
                    },
                    new()
                    {
                        Key = "customerName",
                        Label = "Customer"
                    },
                    new()
                    {
                        Key = "customerNumber",
                        Label = "Mobile"
                    },
                    new()
                    {
                        Key = "cancellationDate",
                        Label = "Cancellation Date"
                    },
                    new()
                    {
                        Key = "cancellationReason",
                        Label = "Cancellation Reason"
                    },
                    new()
                    {
                        Key = "employee",
                        Label = "Employee"
                    },
                    new()
                    {
                        Key = "agency",
                        Label = "Agency"
                    },
                    new()
                    {
                        Key = "insuranceType",
                        Label = "Insurance Type"
                    }
                ],

                Metrics =
                [
                    new()
                    {
                        Key = "totalCancelled",
                        Label = "Total Cancelled",
                        Value = totalCount,
                        Format = "number"
                    }
                ],

                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion
    }
}