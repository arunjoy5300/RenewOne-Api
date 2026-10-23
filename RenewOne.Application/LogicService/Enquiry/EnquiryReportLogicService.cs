using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Reports;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Enquiry
{
    public sealed class EnquiryReportLogicService : IEnquiryReportLogicService
    {
        private const int MaxExportRecords = 25000;
        private readonly IEnquiryReportRepository _repository;

        public EnquiryReportLogicService(IEnquiryReportRepository repository)
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
                EnquiryReportType.Status => await GenerateStatusReportAsync(request, cancellationToken),
                EnquiryReportType.Cancelled => await GenerateCancelledReportAsync(request, cancellationToken),
                EnquiryReportType.Pending => await GeneratePendingReportAsync(request, cancellationToken),
                EnquiryReportType.VehicleType => await GenerateVehicleTypeReportAsync(request, cancellationToken),
                EnquiryReportType.InsuranceType => await GenerateInsuranceTypeReportAsync(request, cancellationToken),
                EnquiryReportType.ConvertedPolicies => await GenerateConvertedPoliciesReportAsync(request, cancellationToken),
                EnquiryReportType.EmployeePerformance => await GenerateEmployeePerformanceReportAsync(request, cancellationToken),
                EnquiryReportType.AgencyPerformance => await GenerateAgencyPerformanceReportAsync(request, cancellationToken),
                EnquiryReportType.InsuranceCompany => await GenerateInsuranceCompanyReportAsync(request, cancellationToken),
                EnquiryReportType.DailyMonthly => await GenerateDailyMonthlyReportAsync(request, cancellationToken),
                EnquiryReportType.ProfitAnalysis => await GenerateProfitAnalysisReportAsync(request, cancellationToken),
                EnquiryReportType.DocumentPending => await GenerateDocumentPendingReportAsync(request, cancellationToken),
                _ => throw new NotSupportedException($"Report type '{request.ReportType}' is not supported.")
            };
        }

        #region Common Filter

        private static IQueryable<RenewOne.Domain.Entities.Enquiry> ApplyCommonFilters(
            IQueryable<RenewOne.Domain.Entities.Enquiry> query,
            EnquiryReportFilter filter)
        {
            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDateExclusive = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedAt < toDateExclusive);
            }

            if (filter.Statuses is { Count: > 0 })
            {
                query = query.Where(x => filter.Statuses.Contains(x.Status));
            }

            if (filter.EmployeeIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.AssignedEmployeeId.HasValue &&
                    filter.EmployeeIds.Contains(x.AssignedEmployeeId.Value));
            }

            if (filter.AgencyIds is { Count: > 0 })
            {
                query = query.Where(x => filter.AgencyIds.Contains(x.TenantId));
            }

            if (filter.InsuranceTypeIds is { Count: > 0 })
            {
                query = query.Where(x => filter.InsuranceTypeIds.Contains(x.InsuranceTypeId));
            }

            if (filter.InsuranceCompanyIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.EnquiryQuotes.Any(q => filter.InsuranceCompanyIds.Contains(q.InsuranceCompanyId)));
            }

            if (filter.VehicleTypeIds is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.VehicleEnquiryDetails != null &&
                    filter.VehicleTypeIds.Contains(x.VehicleEnquiryDetails.VehicleTypeId));
            }

            if (!string.IsNullOrWhiteSpace(filter.EnquiryNumber))
            {
                query = query.Where(x =>
                    (x.EnquiryNumber != null && x.EnquiryNumber.Contains(filter.EnquiryNumber)) ||
                    x.Id.ToString().Contains(filter.EnquiryNumber));
            }

            if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            {
                query = query.Where(x => x.CustomerName.Contains(filter.CustomerName));
            }

            return query;
        }

        private static (int skip, int take) GetPagination(EnquiryReportRequest request)
        {
            if (request.ExportAll)
            {
                return (0, MaxExportRecords);
            }

            return ((request.PageNumber - 1) * request.PageSize, request.PageSize);
        }

        #endregion

        #region 1. Status Report

        private async Task<EnquiryReportResult> GenerateStatusReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var totalCount = await query.CountAsync(cancellationToken);

            var totalPremium = await query
                .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium))
                .SumAsync(cancellationToken) ?? 0m;

            var totalProfit = await query
                .Where(x => x.Status == "PolicyIssued")
                .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit))
                .SumAsync(cancellationToken) ?? 0m;

            var (skip, take) = GetPagination(request);

            var rawRows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    EnquiryNumber = !string.IsNullOrEmpty(x.EnquiryNumber) ? x.EnquiryNumber : x.Id.ToString(),
                    x.CustomerName,
                    x.CustomerNumber,
                    x.Status,
                    x.CreatedAt,
                    Employee = x.AssignedEmployee != null ? x.AssignedEmployee.FullName : null,
                    Agency = x.Tenant != null ? x.Tenant.AgentName : null,
                    InsuranceType = x.InsuranceType != null ? x.InsuranceType.Name : null
                })
                .ToListAsync(cancellationToken);

            var rows = rawRows.Select(x => (IReadOnlyDictionary<string, object?>)
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
                }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.Status,
                Columns =
                [
                    new() { Key = "enquiryNumber", Label = "Enquiry No" },
                    new() { Key = "customerName", Label = "Customer" },
                    new() { Key = "customerNumber", Label = "Mobile" },
                    new() { Key = "status", Label = "Status" },
                    new() { Key = "createdAt", Label = "Created Date" },
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "agency", Label = "Agency" },
                    new() { Key = "insuranceType", Label = "Insurance Type" }
                ],
                Metrics =
                [
                    new() { Key = "totalEnquiries", Label = "Total Enquiries", Value = totalCount, Format = "number" },
                    new() { Key = "totalPremium", Label = "Total Premium", Value = totalPremium, Format = "currency" },
                    new() { Key = "totalProfit", Label = "Total Profit", Value = totalProfit, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 2. Cancelled Report

        private async Task<EnquiryReportResult> GenerateCancelledReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var baseQuery = _repository.GetEnquiries();
            var totalAllPeriod = await baseQuery.CountAsync(cancellationToken);

            var query = ApplyCommonFilters(baseQuery, request.Filter)
                .Where(x => x.Status == "Cancelled");

            if (request.Filter.CancellationFromDate.HasValue)
            {
                var fromDate = request.Filter.CancellationFromDate.Value.Date;
                query = query.Where(x => x.CancellationDate.HasValue && x.CancellationDate.Value >= fromDate);
            }

            if (request.Filter.CancellationToDate.HasValue)
            {
                var toDateExclusive = request.Filter.CancellationToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.CancellationDate.HasValue && x.CancellationDate.Value < toDateExclusive);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var cancellationRate = totalAllPeriod > 0
                ? Math.Round((totalCount / (decimal)totalAllPeriod) * 100m, 2)
                : 0m;

            var (skip, take) = GetPagination(request);

            var rawRows = await query
                .OrderByDescending(x => x.CancellationDate ?? x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    EnquiryNumber = !string.IsNullOrEmpty(x.EnquiryNumber) ? x.EnquiryNumber : x.Id.ToString(),
                    x.CustomerName,
                    x.CustomerNumber,
                    CancellationDate = x.CancellationDate ?? x.CreatedAt,
                    CancellationReason = x.CancellationReason ?? "Not specified",
                    Employee = x.AssignedEmployee != null ? x.AssignedEmployee.FullName : null,
                    Agency = x.Tenant != null ? x.Tenant.AgentName : null,
                    InsuranceType = x.InsuranceType != null ? x.InsuranceType.Name : null
                })
                .ToListAsync(cancellationToken);

            var rows = rawRows.Select(x => (IReadOnlyDictionary<string, object?>)
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
                }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.Cancelled,
                Columns =
                [
                    new() { Key = "enquiryNumber", Label = "Enquiry No" },
                    new() { Key = "customerName", Label = "Customer" },
                    new() { Key = "customerNumber", Label = "Mobile" },
                    new() { Key = "cancellationDate", Label = "Cancellation Date" },
                    new() { Key = "cancellationReason", Label = "Cancellation Reason" },
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "agency", Label = "Agency" },
                    new() { Key = "insuranceType", Label = "Insurance Type" }
                ],
                Metrics =
                [
                    new() { Key = "totalCancelled", Label = "Total Cancelled", Value = totalCount, Format = "number" },
                    new() { Key = "cancellationRate", Label = "Cancellation Rate", Value = cancellationRate, Format = "percentage" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 3. Pending Report

        private async Task<EnquiryReportResult> GeneratePendingReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter)
                .Where(x => x.Status != "PolicyIssued" && x.Status != "Cancelled");

            if (request.Filter.PendingStageIds is { Count: > 0 })
            {
                query = query.Where(x => x.CurrentStep.HasValue && request.Filter.PendingStageIds.Contains(x.CurrentStep.Value));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var (skip, take) = GetPagination(request);

            var now = DateTime.UtcNow;

            var rawRows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    EnquiryNumber = !string.IsNullOrEmpty(x.EnquiryNumber) ? x.EnquiryNumber : x.Id.ToString(),
                    x.CustomerName,
                    x.CustomerNumber,
                    x.Status,
                    PendingStep = x.CurrentStep ?? 1,
                    x.CreatedAt,
                    Employee = x.AssignedEmployee != null ? x.AssignedEmployee.FullName : null,
                    Agency = x.Tenant != null ? x.Tenant.AgentName : null,
                    InsuranceType = x.InsuranceType != null ? x.InsuranceType.Name : null
                })
                .ToListAsync(cancellationToken);

            var rows = rawRows.Select(x =>
            {
                var ageingDays = Math.Max(0, (now - x.CreatedAt).Days);
                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["enquiryNumber"] = x.EnquiryNumber,
                        ["customerName"] = x.CustomerName,
                        ["customerNumber"] = x.CustomerNumber,
                        ["status"] = x.Status,
                        ["currentStep"] = $"Step {x.PendingStep}",
                        ["createdAt"] = x.CreatedAt,
                        ["ageingDays"] = ageingDays,
                        ["employee"] = x.Employee,
                        ["agency"] = x.Agency,
                        ["insuranceType"] = x.InsuranceType
                    };
            }).ToList();

            var avgPendingDays = rows.Count > 0
                ? Math.Round(rows.Average(r => Convert.ToDouble(r["ageingDays"])), 1)
                : 0.0;

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.Pending,
                Columns =
                [
                    new() { Key = "enquiryNumber", Label = "Enquiry No" },
                    new() { Key = "customerName", Label = "Customer" },
                    new() { Key = "customerNumber", Label = "Mobile" },
                    new() { Key = "status", Label = "Status" },
                    new() { Key = "currentStep", Label = "Pending Stage" },
                    new() { Key = "createdAt", Label = "Created Date" },
                    new() { Key = "ageingDays", Label = "Ageing (Days)" },
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "agency", Label = "Agency" },
                    new() { Key = "insuranceType", Label = "Insurance Type" }
                ],
                Metrics =
                [
                    new() { Key = "totalPending", Label = "Total Pending", Value = totalCount, Format = "number" },
                    new() { Key = "averagePendingDays", Label = "Average Pending Days", Value = avgPendingDays, Format = "number" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 4. Vehicle Type Report

        private async Task<EnquiryReportResult> GenerateVehicleTypeReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var groupedQuery = query
                .GroupBy(x => x.VehicleEnquiryDetails != null && x.VehicleEnquiryDetails.VehicleType != null
                    ? x.VehicleEnquiryDetails.VehicleType.TypeName
                    : "Not Specified")
                .Select(g => new
                {
                    VehicleType = g.Key,
                    TotalEnquiries = g.Count(),
                    ConvertedPolicies = g.Count(x => x.Status == "PolicyIssued"),
                    TotalPremium = g.SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium)).Sum() ?? 0m
                });

            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var (skip, take) = GetPagination(request);

            var rawRows = await groupedQuery
                .OrderByDescending(x => x.TotalEnquiries)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var totalEnquiriesOverall = rawRows.Sum(x => x.TotalEnquiries);
            var totalConvertedOverall = rawRows.Sum(x => x.ConvertedPolicies);
            var totalPremiumOverall = rawRows.Sum(x => x.TotalPremium);
            var overallConversionRate = totalEnquiriesOverall > 0
                ? Math.Round((totalConvertedOverall / (decimal)totalEnquiriesOverall) * 100m, 1)
                : 0m;

            var rows = rawRows.Select(x =>
            {
                var rate = x.TotalEnquiries > 0
                    ? Math.Round((x.ConvertedPolicies / (decimal)x.TotalEnquiries) * 100m, 1)
                    : 0m;

                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["vehicleType"] = x.VehicleType,
                        ["totalEnquiries"] = x.TotalEnquiries,
                        ["convertedPolicies"] = x.ConvertedPolicies,
                        ["conversionRate"] = $"{rate}%",
                        ["totalPremium"] = x.TotalPremium
                    };
            }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.VehicleType,
                Columns =
                [
                    new() { Key = "vehicleType", Label = "Vehicle Type" },
                    new() { Key = "totalEnquiries", Label = "Total Enquiries" },
                    new() { Key = "convertedPolicies", Label = "Converted Policies" },
                    new() { Key = "conversionRate", Label = "Conversion Rate" },
                    new() { Key = "totalPremium", Label = "Total Premium" }
                ],
                Metrics =
                [
                    new() { Key = "totalEnquiries", Label = "Total Enquiries", Value = totalEnquiriesOverall, Format = "number" },
                    new() { Key = "convertedPolicies", Label = "Converted Policies", Value = totalConvertedOverall, Format = "number" },
                    new() { Key = "conversionRate", Label = "Conversion Rate", Value = overallConversionRate, Format = "percentage" },
                    new() { Key = "totalPremium", Label = "Total Premium", Value = totalPremiumOverall, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 5. Insurance Type Report

        private async Task<EnquiryReportResult> GenerateInsuranceTypeReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var groupedQuery = query
                .GroupBy(x => x.InsuranceType != null ? x.InsuranceType.Name : "General")
                .Select(g => new
                {
                    InsuranceType = g.Key,
                    TotalEnquiries = g.Count(),
                    ConvertedPolicies = g.Count(x => x.Status == "PolicyIssued"),
                    TotalPremium = g.SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium)).Sum() ?? 0m,
                    TotalProfit = g.Where(x => x.Status == "PolicyIssued")
                        .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit)).Sum() ?? 0m
                });

            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var (skip, take) = GetPagination(request);

            var rawRows = await groupedQuery
                .OrderByDescending(x => x.TotalEnquiries)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var sumEnquiries = rawRows.Sum(x => x.TotalEnquiries);
            var sumConverted = rawRows.Sum(x => x.ConvertedPolicies);
            var sumPremium = rawRows.Sum(x => x.TotalPremium);
            var sumProfit = rawRows.Sum(x => x.TotalProfit);
            var overallRate = sumEnquiries > 0
                ? Math.Round((sumConverted / (decimal)sumEnquiries) * 100m, 1)
                : 0m;

            var rows = rawRows.Select(x =>
            {
                var rate = x.TotalEnquiries > 0
                    ? Math.Round((x.ConvertedPolicies / (decimal)x.TotalEnquiries) * 100m, 1)
                    : 0m;

                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["insuranceType"] = x.InsuranceType,
                        ["totalEnquiries"] = x.TotalEnquiries,
                        ["convertedPolicies"] = x.ConvertedPolicies,
                        ["conversionRate"] = $"{rate}%",
                        ["totalPremium"] = x.TotalPremium,
                        ["totalProfit"] = x.TotalProfit
                    };
            }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.InsuranceType,
                Columns =
                [
                    new() { Key = "insuranceType", Label = "Insurance Type" },
                    new() { Key = "totalEnquiries", Label = "Total Enquiries" },
                    new() { Key = "convertedPolicies", Label = "Converted Policies" },
                    new() { Key = "conversionRate", Label = "Conversion Rate" },
                    new() { Key = "totalPremium", Label = "Total Premium" },
                    new() { Key = "totalProfit", Label = "Total Profit" }
                ],
                Metrics =
                [
                    new() { Key = "totalEnquiries", Label = "Total Enquiries", Value = sumEnquiries, Format = "number" },
                    new() { Key = "convertedPolicies", Label = "Converted Policies", Value = sumConverted, Format = "number" },
                    new() { Key = "conversionRate", Label = "Conversion Rate", Value = overallRate, Format = "percentage" },
                    new() { Key = "totalPremium", Label = "Total Premium", Value = sumPremium, Format = "currency" },
                    new() { Key = "totalProfit", Label = "Total Profit", Value = sumProfit, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 6. Converted Policies Report

        private async Task<EnquiryReportResult> GenerateConvertedPoliciesReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter)
                .Where(x => x.Status == "PolicyIssued" || x.PolicyDetails != null);

            var totalCount = await query.CountAsync(cancellationToken);

            var totalPremium = await query
                .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium))
                .SumAsync(cancellationToken) ?? 0m;

            var totalProfit = await query
                .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit))
                .SumAsync(cancellationToken) ?? 0m;

            var avgPremium = totalCount > 0 ? Math.Round(totalPremium / totalCount, 2) : 0m;
            var avgProfit = totalCount > 0 ? Math.Round(totalProfit / totalCount, 2) : 0m;

            var (skip, take) = GetPagination(request);

            var rawRows = await query
                .OrderByDescending(x => x.PolicyDetails != null ? x.PolicyDetails.CreatedAt : x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    EnquiryNumber = !string.IsNullOrEmpty(x.EnquiryNumber) ? x.EnquiryNumber : x.Id.ToString(),
                    x.CustomerName,
                    x.CustomerNumber,
                    PolicyNumber = x.PolicyDetails != null ? x.PolicyDetails.PolicyNumber : "Issued",
                    InsuranceCompany = x.EnquiryQuotes.Where(q => q.IsSelected && q.InsuranceCompany != null).Select(q => q.InsuranceCompany!.CompanyName).FirstOrDefault() ?? "Direct",
                    Premium = x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium).FirstOrDefault() ?? 0m,
                    Profit = x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit).FirstOrDefault() ?? 0m,
                    ConvertedDate = x.PolicyDetails != null ? x.PolicyDetails.CreatedAt : x.CreatedAt,
                    Employee = x.AssignedEmployee != null ? x.AssignedEmployee.FullName : null,
                    Agency = x.Tenant != null ? x.Tenant.AgentName : null,
                    InsuranceType = x.InsuranceType != null ? x.InsuranceType.Name : null
                })
                .ToListAsync(cancellationToken);

            var rows = rawRows.Select(x => (IReadOnlyDictionary<string, object?>)
                new Dictionary<string, object?>
                {
                    ["enquiryNumber"] = x.EnquiryNumber,
                    ["customerName"] = x.CustomerName,
                    ["customerNumber"] = x.CustomerNumber,
                    ["policyNumber"] = x.PolicyNumber,
                    ["insuranceCompany"] = x.InsuranceCompany,
                    ["premium"] = x.Premium,
                    ["adminProfit"] = x.Profit,
                    ["employee"] = x.Employee,
                    ["agency"] = x.Agency,
                    ["insuranceType"] = x.InsuranceType,
                    ["convertedDate"] = x.ConvertedDate
                }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.ConvertedPolicies,
                Columns =
                [
                    new() { Key = "enquiryNumber", Label = "Enquiry No" },
                    new() { Key = "customerName", Label = "Customer" },
                    new() { Key = "customerNumber", Label = "Mobile" },
                    new() { Key = "policyNumber", Label = "Policy No" },
                    new() { Key = "insuranceCompany", Label = "Insurance Company" },
                    new() { Key = "premium", Label = "Premium" },
                    new() { Key = "adminProfit", Label = "Admin Profit" },
                    new() { Key = "employee", Label = "Issued By" },
                    new() { Key = "agency", Label = "Agency" },
                    new() { Key = "insuranceType", Label = "Insurance Type" },
                    new() { Key = "convertedDate", Label = "Converted Date" }
                ],
                Metrics =
                [
                    new() { Key = "totalPolicies", Label = "Total Policies", Value = totalCount, Format = "number" },
                    new() { Key = "totalPremium", Label = "Total Premium", Value = totalPremium, Format = "currency" },
                    new() { Key = "totalProfit", Label = "Total Profit", Value = totalProfit, Format = "currency" },
                    new() { Key = "averagePremium", Label = "Average Premium", Value = avgPremium, Format = "currency" },
                    new() { Key = "averageProfit", Label = "Average Profit", Value = avgProfit, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 7. Employee Performance Report

        private async Task<EnquiryReportResult> GenerateEmployeePerformanceReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var groupedQuery = query
                .Where(x => x.AssignedEmployee != null)
                .GroupBy(x => x.AssignedEmployee!.FullName)
                .Select(g => new
                {
                    Employee = g.Key,
                    TotalAssigned = g.Count(),
                    TotalQuoted = g.Count(x => x.EnquiryQuotes.Any()),
                    TotalConverted = g.Count(x => x.Status == "PolicyIssued"),
                    TotalPremium = g.SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium)).Sum() ?? 0m,
                    TotalProfit = g.Where(x => x.Status == "PolicyIssued")
                        .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit)).Sum() ?? 0m
                });

            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var (skip, take) = GetPagination(request);

            var rawRows = await groupedQuery
                .OrderByDescending(x => x.TotalConverted)
                .ThenByDescending(x => x.TotalAssigned)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var sumAssigned = rawRows.Sum(x => x.TotalAssigned);
            var sumQuoted = rawRows.Sum(x => x.TotalQuoted);
            var sumConverted = rawRows.Sum(x => x.TotalConverted);
            var sumPremium = rawRows.Sum(x => x.TotalPremium);
            var sumProfit = rawRows.Sum(x => x.TotalProfit);
            var overallRate = sumAssigned > 0
                ? Math.Round((sumConverted / (decimal)sumAssigned) * 100m, 1)
                : 0m;

            var rows = rawRows.Select(x =>
            {
                var rate = x.TotalAssigned > 0
                    ? Math.Round((x.TotalConverted / (decimal)x.TotalAssigned) * 100m, 1)
                    : 0m;

                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["employee"] = x.Employee,
                        ["totalAssigned"] = x.TotalAssigned,
                        ["totalQuoted"] = x.TotalQuoted,
                        ["totalConverted"] = x.TotalConverted,
                        ["conversionRate"] = $"{rate}%",
                        ["totalPremium"] = x.TotalPremium,
                        ["totalProfit"] = x.TotalProfit
                    };
            }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.EmployeePerformance,
                Columns =
                [
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "totalAssigned", Label = "Assigned" },
                    new() { Key = "totalQuoted", Label = "Quoted" },
                    new() { Key = "totalConverted", Label = "Converted" },
                    new() { Key = "conversionRate", Label = "Conversion Rate" },
                    new() { Key = "totalPremium", Label = "Total Premium" },
                    new() { Key = "totalProfit", Label = "Total Profit" }
                ],
                Metrics =
                [
                    new() { Key = "totalAssigned", Label = "Total Assigned", Value = sumAssigned, Format = "number" },
                    new() { Key = "totalQuoted", Label = "Total Quoted", Value = sumQuoted, Format = "number" },
                    new() { Key = "totalConverted", Label = "Total Converted", Value = sumConverted, Format = "number" },
                    new() { Key = "conversionRate", Label = "Conversion Rate", Value = overallRate, Format = "percentage" },
                    new() { Key = "totalPremium", Label = "Total Premium Generated", Value = sumPremium, Format = "currency" },
                    new() { Key = "totalProfit", Label = "Total Profit Contribution", Value = sumProfit, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 8. Agency Performance Report

        private async Task<EnquiryReportResult> GenerateAgencyPerformanceReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var groupedQuery = query
                .Where(x => x.Tenant != null)
                .GroupBy(x => x.Tenant!.AgentName)
                .Select(g => new
                {
                    Agency = g.Key,
                    TotalEnquiries = g.Count(),
                    TotalConverted = g.Count(x => x.Status == "PolicyIssued"),
                    TotalPremium = g.SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium)).Sum() ?? 0m,
                    TotalCommission = g.Where(x => x.Status == "PolicyIssued")
                        .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AgentPoints)).Sum() ?? 0m
                });

            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var (skip, take) = GetPagination(request);

            var rawRows = await groupedQuery
                .OrderByDescending(x => x.TotalConverted)
                .ThenByDescending(x => x.TotalPremium)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var sumEnquiries = rawRows.Sum(x => x.TotalEnquiries);
            var sumConverted = rawRows.Sum(x => x.TotalConverted);
            var sumPremium = rawRows.Sum(x => x.TotalPremium);
            var sumCommission = rawRows.Sum(x => x.TotalCommission);
            var overallRate = sumEnquiries > 0
                ? Math.Round((sumConverted / (decimal)sumEnquiries) * 100m, 1)
                : 0m;

            var rows = rawRows.Select(x =>
            {
                var rate = x.TotalEnquiries > 0
                    ? Math.Round((x.TotalConverted / (decimal)x.TotalEnquiries) * 100m, 1)
                    : 0m;

                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["agency"] = x.Agency,
                        ["totalEnquiries"] = x.TotalEnquiries,
                        ["totalConverted"] = x.TotalConverted,
                        ["conversionRate"] = $"{rate}%",
                        ["totalPremium"] = x.TotalPremium,
                        ["totalCommission"] = x.TotalCommission
                    };
            }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.AgencyPerformance,
                Columns =
                [
                    new() { Key = "agency", Label = "Agency" },
                    new() { Key = "totalEnquiries", Label = "Total Enquiries" },
                    new() { Key = "totalConverted", Label = "Total Converted" },
                    new() { Key = "conversionRate", Label = "Conversion Rate" },
                    new() { Key = "totalPremium", Label = "Premium Volume" },
                    new() { Key = "totalCommission", Label = "Commission Paid" }
                ],
                Metrics =
                [
                    new() { Key = "totalEnquiries", Label = "Total Enquiries", Value = sumEnquiries, Format = "number" },
                    new() { Key = "totalConverted", Label = "Total Converted", Value = sumConverted, Format = "number" },
                    new() { Key = "conversionRate", Label = "Conversion Rate", Value = overallRate, Format = "percentage" },
                    new() { Key = "totalPremium", Label = "Premium Volume", Value = sumPremium, Format = "currency" },
                    new() { Key = "totalCommission", Label = "Commission Paid", Value = sumCommission, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 9. Insurance Company Report

        private async Task<EnquiryReportResult> GenerateInsuranceCompanyReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var quotesQuery = query
                .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected && q.InsuranceCompany != null))
                .GroupBy(q => q.InsuranceCompany!.CompanyName)
                .Select(g => new
                {
                    InsuranceCompany = g.Key,
                    PoliciesIssued = g.Count(q => q.Enquiry != null && q.Enquiry.Status == "PolicyIssued"),
                    GrossPremium = g.Sum(q => (decimal?)q.Premium) ?? 0m,
                    InsurerPayable = g.Sum(q => (decimal?)q.CompanyPayable) ?? 0m,
                    AdminProfit = g.Where(q => q.Enquiry != null && q.Enquiry.Status == "PolicyIssued").Sum(q => (decimal?)q.AdminProfit) ?? 0m
                });

            var totalCount = await quotesQuery.CountAsync(cancellationToken);
            var (skip, take) = GetPagination(request);

            var rawRows = await quotesQuery
                .OrderByDescending(x => x.GrossPremium)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var sumPolicies = rawRows.Sum(x => x.PoliciesIssued);
            var sumGross = rawRows.Sum(x => x.GrossPremium);
            var sumPayable = rawRows.Sum(x => x.InsurerPayable);
            var sumProfit = rawRows.Sum(x => x.AdminProfit);

            var rows = rawRows.Select(x => (IReadOnlyDictionary<string, object?>)
                new Dictionary<string, object?>
                {
                    ["insuranceCompany"] = x.InsuranceCompany,
                    ["policiesIssued"] = x.PoliciesIssued,
                    ["grossPremium"] = x.GrossPremium,
                    ["insurerPayable"] = x.InsurerPayable,
                    ["adminProfit"] = x.AdminProfit
                }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.InsuranceCompany,
                Columns =
                [
                    new() { Key = "insuranceCompany", Label = "Insurance Company" },
                    new() { Key = "policiesIssued", Label = "Policies Issued" },
                    new() { Key = "grossPremium", Label = "Gross Premium" },
                    new() { Key = "insurerPayable", Label = "Insurer Payable" },
                    new() { Key = "adminProfit", Label = "Admin Profit" }
                ],
                Metrics =
                [
                    new() { Key = "policiesIssued", Label = "Policies Issued", Value = sumPolicies, Format = "number" },
                    new() { Key = "grossPremium", Label = "Gross Premium", Value = sumGross, Format = "currency" },
                    new() { Key = "insurerPayable", Label = "Insurer Payable", Value = sumPayable, Format = "currency" },
                    new() { Key = "adminProfit", Label = "Admin Profit", Value = sumProfit, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 10. Daily / Monthly Report

        private async Task<EnquiryReportResult> GenerateDailyMonthlyReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter);

            var groupByMonth = string.Equals(request.Filter.GroupBy, "month", StringComparison.OrdinalIgnoreCase);

            var groupedQuery = groupByMonth
                ? query.GroupBy(x => new { Year = x.CreatedAt.Year, Month = x.CreatedAt.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalEnquiries = g.Count(),
                        PoliciesIssued = g.Count(x => x.Status == "PolicyIssued"),
                        GrossPremium = g.SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium)).Sum() ?? 0m,
                        NetProfit = g.Where(x => x.Status == "PolicyIssued")
                            .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit)).Sum() ?? 0m
                    })
                : query.GroupBy(x => x.CreatedAt.Date)
                    .Select(g => new
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        TotalEnquiries = g.Count(),
                        PoliciesIssued = g.Count(x => x.Status == "PolicyIssued"),
                        GrossPremium = g.SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium)).Sum() ?? 0m,
                        NetProfit = g.Where(x => x.Status == "PolicyIssued")
                            .SelectMany(x => x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit)).Sum() ?? 0m
                    });

            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var (skip, take) = GetPagination(request);

            var rawRows = await groupedQuery
                .OrderByDescending(x => x.Period)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            var sumEnquiries = rawRows.Sum(x => x.TotalEnquiries);
            var sumIssued = rawRows.Sum(x => x.PoliciesIssued);
            var sumGross = rawRows.Sum(x => x.GrossPremium);
            var sumProfit = rawRows.Sum(x => x.NetProfit);

            var rows = rawRows.Select(x => (IReadOnlyDictionary<string, object?>)
                new Dictionary<string, object?>
                {
                    ["period"] = x.Period,
                    ["totalEnquiries"] = x.TotalEnquiries,
                    ["totalPoliciesIssued"] = x.PoliciesIssued,
                    ["grossPremium"] = x.GrossPremium,
                    ["netProfit"] = x.NetProfit
                }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.DailyMonthly,
                Columns =
                [
                    new() { Key = "period", Label = groupByMonth ? "Month" : "Date" },
                    new() { Key = "totalEnquiries", Label = "Total Enquiries" },
                    new() { Key = "totalPoliciesIssued", Label = "Policies Issued" },
                    new() { Key = "grossPremium", Label = "Gross Premium" },
                    new() { Key = "netProfit", Label = "Net Profit" }
                ],
                Metrics =
                [
                    new() { Key = "totalEnquiries", Label = "Total Enquiries", Value = sumEnquiries, Format = "number" },
                    new() { Key = "totalPoliciesIssued", Label = "Policies Issued", Value = sumIssued, Format = "number" },
                    new() { Key = "grossPremium", Label = "Gross Premium", Value = sumGross, Format = "currency" },
                    new() { Key = "netProfit", Label = "Net Profit", Value = sumProfit, Format = "currency" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 11. Profit Analysis Report

        private async Task<EnquiryReportResult> GenerateProfitAnalysisReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter)
                .Where(x => x.EnquiryQuotes.Any(q => q.IsSelected));

            var totalCount = await query.CountAsync(cancellationToken);

            var (skip, take) = GetPagination(request);

            var rawRows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    EnquiryNumber = !string.IsNullOrEmpty(x.EnquiryNumber) ? x.EnquiryNumber : x.Id.ToString(),
                    x.CustomerName,
                    InsuranceCompany = x.EnquiryQuotes.Where(q => q.IsSelected && q.InsuranceCompany != null).Select(q => q.InsuranceCompany!.CompanyName).FirstOrDefault() ?? "Direct",
                    GrossPremium = x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.Premium).FirstOrDefault() ?? 0m,
                    InsurerCost = x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.CompanyPayable).FirstOrDefault() ?? 0m,
                    CscCommission = x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AgentPoints).FirstOrDefault() ?? 0m,
                    NetAdminProfit = x.EnquiryQuotes.Where(q => q.IsSelected).Select(q => (decimal?)q.AdminProfit).FirstOrDefault() ?? 0m,
                    Employee = x.AssignedEmployee != null ? x.AssignedEmployee.FullName : null,
                    Agency = x.Tenant != null ? x.Tenant.AgentName : null
                })
                .ToListAsync(cancellationToken);

            var sumGross = rawRows.Sum(x => x.GrossPremium);
            var sumInsurerCost = rawRows.Sum(x => x.InsurerCost);
            var sumCommission = rawRows.Sum(x => x.CscCommission);
            var sumNetProfit = rawRows.Sum(x => x.NetAdminProfit);
            var avgMargin = sumGross > 0 ? Math.Round((sumNetProfit / sumGross) * 100m, 1) : 0m;

            var rows = rawRows.Select(x =>
            {
                var margin = x.GrossPremium > 0 ? Math.Round((x.NetAdminProfit / x.GrossPremium) * 100m, 1) : 0m;

                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["enquiryNumber"] = x.EnquiryNumber,
                        ["customerName"] = x.CustomerName,
                        ["insuranceCompany"] = x.InsuranceCompany,
                        ["grossPremium"] = x.GrossPremium,
                        ["insurerCost"] = x.InsurerCost,
                        ["cscCommission"] = x.CscCommission,
                        ["netAdminProfit"] = x.NetAdminProfit,
                        ["marginPercentage"] = $"{margin}%",
                        ["employee"] = x.Employee,
                        ["agency"] = x.Agency
                    };
            }).ToList();

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.ProfitAnalysis,
                Columns =
                [
                    new() { Key = "enquiryNumber", Label = "Enquiry No" },
                    new() { Key = "customerName", Label = "Customer" },
                    new() { Key = "insuranceCompany", Label = "Insurer" },
                    new() { Key = "grossPremium", Label = "Gross Premium" },
                    new() { Key = "insurerCost", Label = "Insurer Cost" },
                    new() { Key = "cscCommission", Label = "CSC Commission" },
                    new() { Key = "netAdminProfit", Label = "Net Admin Profit" },
                    new() { Key = "marginPercentage", Label = "Margin (%)" },
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "agency", Label = "Agency" }
                ],
                Metrics =
                [
                    new() { Key = "grossPremium", Label = "Gross Premium", Value = sumGross, Format = "currency" },
                    new() { Key = "insurerCost", Label = "Insurer Cost", Value = sumInsurerCost, Format = "currency" },
                    new() { Key = "cscCommission", Label = "CSC Commission", Value = sumCommission, Format = "currency" },
                    new() { Key = "netAdminProfit", Label = "Net Admin Profit", Value = sumNetProfit, Format = "currency" },
                    new() { Key = "averageMargin", Label = "Average Margin", Value = avgMargin, Format = "percentage" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion

        #region 12. Document Pending Report

        private async Task<EnquiryReportResult> GenerateDocumentPendingReportAsync(
            EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var query = ApplyCommonFilters(_repository.GetEnquiries(), request.Filter)
                .Where(x => x.Status != "Cancelled" && x.Status != "PolicyIssued");

            var totalCount = await query.CountAsync(cancellationToken);

            var (skip, take) = GetPagination(request);
            var now = DateTime.UtcNow;

            var rawRows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new
                {
                    EnquiryNumber = !string.IsNullOrEmpty(x.EnquiryNumber) ? x.EnquiryNumber : x.Id.ToString(),
                    x.CustomerName,
                    x.CustomerNumber,
                    x.Status,
                    UploadedCount = x.Documents.Count(d => d.IsActive),
                    x.CreatedAt,
                    Employee = x.AssignedEmployee != null ? x.AssignedEmployee.FullName : null,
                    Agency = x.Tenant != null ? x.Tenant.AgentName : null,
                    InsuranceType = x.InsuranceType != null ? x.InsuranceType.Name : null
                })
                .ToListAsync(cancellationToken);

            var rows = rawRows.Select(x =>
            {
                var ageing = Math.Max(0, (now - x.CreatedAt).Days);
                return (IReadOnlyDictionary<string, object?>)
                    new Dictionary<string, object?>
                    {
                        ["enquiryNumber"] = x.EnquiryNumber,
                        ["customerName"] = x.CustomerName,
                        ["customerNumber"] = x.CustomerNumber,
                        ["status"] = x.Status,
                        ["uploadedCount"] = x.UploadedCount,
                        ["pendingAgeingDays"] = ageing,
                        ["employee"] = x.Employee,
                        ["agency"] = x.Agency,
                        ["insuranceType"] = x.InsuranceType
                    };
            }).ToList();

            var avgAgeing = rows.Count > 0
                ? Math.Round(rows.Average(r => Convert.ToDouble(r["pendingAgeingDays"])), 1)
                : 0.0;

            var estimatedMissingDocs = rows.Sum(r => Math.Max(0, 3 - Convert.ToInt32(r["uploadedCount"])));

            var totalPages = request.ExportAll ? 1 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new EnquiryReportResult
            {
                ReportType = EnquiryReportType.DocumentPending,
                Columns =
                [
                    new() { Key = "enquiryNumber", Label = "Enquiry No" },
                    new() { Key = "customerName", Label = "Customer" },
                    new() { Key = "customerNumber", Label = "Mobile" },
                    new() { Key = "status", Label = "Status" },
                    new() { Key = "uploadedCount", Label = "Uploaded Documents" },
                    new() { Key = "pendingAgeingDays", Label = "Ageing (Days)" },
                    new() { Key = "employee", Label = "Employee" },
                    new() { Key = "agency", Label = "Agency" },
                    new() { Key = "insuranceType", Label = "Insurance Type" }
                ],
                Metrics =
                [
                    new() { Key = "totalEnquiries", Label = "Total Enquiries", Value = totalCount, Format = "number" },
                    new() { Key = "totalMissingDocuments", Label = "Pending Documents Count", Value = estimatedMissingDocs, Format = "number" },
                    new() { Key = "averageDaysPending", Label = "Average Days Pending", Value = avgAgeing, Format = "number" }
                ],
                Data = new PagedResponse<IReadOnlyDictionary<string, object?>>
                {
                    Items = rows,
                    PageNumber = request.PageNumber,
                    PageSize = request.ExportAll ? rows.Count : request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = totalPages
                }
            };
        }

        #endregion
    }
}