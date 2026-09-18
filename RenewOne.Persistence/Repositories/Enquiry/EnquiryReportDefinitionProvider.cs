using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Domain.Enums;
using RenewOne.Domain.Reports.Enquiries.Models;

namespace RenewOne.Persistence.Repositories.Enquiry
{
    public sealed class EnquiryReportDefinitionProvider : IEnquiryReportDefinitionProvider
    {
        public IReadOnlyList<EnquiryReportDefinition> GetDefinitions()
        {
            return
            [
                CreateStatusReport(),
                CreateCancelledReport(),
                CreatePendingReport(),
                CreateVehicleTypeReport(),
                CreateInsuranceTypeReport(),
                CreateConvertedPoliciesReport(),
                CreateEmployeePerformanceReport(),
                CreateAgencyPerformanceReport(),
                CreateInsuranceCompanyReport(),
                CreateDailyMonthlyReport(),
                CreateProfitAnalysisReport(),
                CreateDocumentPendingReport()
            ];
        }

        private static EnquiryReportDefinition CreateStatusReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.Status,
                Name = "Enquiry Status Report",
                Description = "Overview of enquiries by status.",

                Filters =
                [
                    new()
                {
                    Key = "dateRange",
                    Label = "Date Range",
                    Type = "dateRange"
                },
                new()
                {
                    Key = "status",
                    Label = "Status",
                    Type = "multiSelect",
                    Multiple = true
                },
                new()
                {
                    Key = "employee",
                    Label = "Employee",
                    Type = "multiSelect",
                    Multiple = true
                },
                new()
                {
                    Key = "agency",
                    Label = "Agency",
                    Type = "multiSelect",
                    Multiple = true
                },
                new()
                {
                    Key = "insuranceType",
                    Label = "Insurance Type",
                    Type = "multiSelect",
                    Multiple = true
                },
                new()
                {
                    Key = "insuranceCompany",
                    Label = "Insurance Company",
                    Type = "multiSelect",
                    Multiple = true
                }
                ],

                Metrics =
                [
                    new()
                {
                    Key = "totalEnquiries",
                    Label = "Total Enquiries",
                    Format = "number"
                },
                new()
                {
                    Key = "totalPremium",
                    Label = "Total Premium",
                    Format = "currency"
                },
                new()
                {
                    Key = "totalProfit",
                    Label = "Total Profit",
                    Format = "currency"
                }
                ]
            };
        }

        private static EnquiryReportDefinition CreateCancelledReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.Cancelled,
                Name = "Cancelled Enquiries",
                Description = "Detailed report of cancelled enquiries.",

                Filters =
                [
                    new()
            {
                Key = "cancellationDateRange",
                Label = "Cancellation Date",
                Type = "dateRange"
            },
            new()
            {
                Key = "cancelledBy",
                Label = "Cancelled By",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "cancellationReason",
                Label = "Cancellation Reason",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalCancelled",
                Label = "Total Cancelled",
                Format = "number"
            },
            new()
            {
                Key = "cancellationRate",
                Label = "Cancellation Rate",
                Format = "percentage"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreatePendingReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.Pending,
                Name = "Pending Enquiries",
                Description = "Report of enquiries that are currently pending.",

                Filters =
                [
                    new()
            {
                Key = "enquiryDateRange",
                Label = "Enquiry Date",
                Type = "dateRange"
            },
            new()
            {
                Key = "pendingStage",
                Label = "Pending Stage",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "ageing",
                Label = "Ageing",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalPending",
                Label = "Total Pending",
                Format = "number"
            },
            new()
            {
                Key = "averagePendingDays",
                Label = "Average Pending Days",
                Format = "number"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateVehicleTypeReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.VehicleType,
                Name = "Vehicle Type Report",
                Description = "Analyse enquiry and conversion performance by vehicle type.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalEnquiries",
                Label = "Total Enquiries",
                Format = "number"
            },
            new()
            {
                Key = "convertedPolicies",
                Label = "Converted Policies",
                Format = "number"
            },
            new()
            {
                Key = "conversionRate",
                Label = "Conversion Rate",
                Format = "percentage"
            },
            new()
            {
                Key = "totalPremium",
                Label = "Total Premium",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateInsuranceTypeReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.InsuranceType,
                Name = "Insurance Type Report",
                Description = "Analyse enquiry and conversion performance by insurance type.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalEnquiries",
                Label = "Total Enquiries",
                Format = "number"
            },
            new()
            {
                Key = "convertedPolicies",
                Label = "Converted Policies",
                Format = "number"
            },
            new()
            {
                Key = "conversionRate",
                Label = "Conversion Rate",
                Format = "percentage"
            },
            new()
            {
                Key = "totalPremium",
                Label = "Total Premium",
                Format = "currency"
            },
            new()
            {
                Key = "totalProfit",
                Label = "Total Profit",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateConvertedPoliciesReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.ConvertedPolicies,
                Name = "Converted Policies Report",
                Description = "Report of enquiries that were successfully converted into policies.",

                Filters =
                [
                    new()
            {
                Key = "convertedDateRange",
                Label = "Converted Date",
                Type = "dateRange"
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Issued By",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalPolicies",
                Label = "Total Policies",
                Format = "number"
            },
            new()
            {
                Key = "totalPremium",
                Label = "Total Premium",
                Format = "currency"
            },
            new()
            {
                Key = "totalProfit",
                Label = "Total Profit",
                Format = "currency"
            },
            new()
            {
                Key = "averagePremium",
                Label = "Average Premium",
                Format = "currency"
            },
            new()
            {
                Key = "averageProfit",
                Label = "Average Profit",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateEmployeePerformanceReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.EmployeePerformance,
                Name = "Employee Performance Report",
                Description = "Measure employee performance based on enquiry and policy conversion activity.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalAssigned",
                Label = "Total Assigned",
                Format = "number"
            },
            new()
            {
                Key = "totalQuoted",
                Label = "Total Quoted",
                Format = "number"
            },
            new()
            {
                Key = "totalConverted",
                Label = "Total Converted",
                Format = "number"
            },
            new()
            {
                Key = "conversionRate",
                Label = "Conversion Rate",
                Format = "percentage"
            },
            new()
            {
                Key = "totalPremium",
                Label = "Total Premium Generated",
                Format = "currency"
            },
            new()
            {
                Key = "totalProfit",
                Label = "Total Profit Contribution",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateAgencyPerformanceReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.AgencyPerformance,
                Name = "Agency Performance Report",
                Description = "Compare enquiry and conversion performance across agencies.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalEnquiries",
                Label = "Total Enquiries",
                Format = "number"
            },
            new()
            {
                Key = "totalConverted",
                Label = "Total Converted",
                Format = "number"
            },
            new()
            {
                Key = "conversionRate",
                Label = "Conversion Rate",
                Format = "percentage"
            },
            new()
            {
                Key = "totalPremium",
                Label = "Premium Volume",
                Format = "currency"
            },
            new()
            {
                Key = "totalCommission",
                Label = "Commission Paid",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateInsuranceCompanyReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.InsuranceCompany,
                Name = "Insurance Company Report",
                Description = "Analyse business generated through each insurance company.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "policiesIssued",
                Label = "Policies Issued",
                Format = "number"
            },
            new()
            {
                Key = "grossPremium",
                Label = "Gross Premium",
                Format = "currency"
            },
            new()
            {
                Key = "insurerPayable",
                Label = "Insurer Payable",
                Format = "currency"
            },
            new()
            {
                Key = "adminProfit",
                Label = "Admin Profit",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateDailyMonthlyReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.DailyMonthly,
                Name = "Daily / Monthly Report",
                Description = "Time-based overview of enquiry and policy business.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "groupBy",
                Label = "Group By",
                Type = "select"
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalEnquiries",
                Label = "Total Enquiries",
                Format = "number"
            },
            new()
            {
                Key = "totalPoliciesIssued",
                Label = "Policies Issued",
                Format = "number"
            },
            new()
            {
                Key = "grossPremium",
                Label = "Gross Premium",
                Format = "currency"
            },
            new()
            {
                Key = "netProfit",
                Label = "Net Profit",
                Format = "currency"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateProfitAnalysisReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.ProfitAnalysis,
                Name = "Profit Analysis Report",
                Description = "Analyse gross premium, insurer cost, commission and net admin profit.",

                Filters =
                [
                    new()
            {
                Key = "dateRange",
                Label = "Date Range",
                Type = "dateRange"
            },
            new()
            {
                Key = "insuranceCompany",
                Label = "Insurance Company",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "insuranceType",
                Label = "Insurance Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "vehicleType",
                Label = "Vehicle Type",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "profitRange",
                Label = "Profit Range",
                Type = "numberRange"
            },
            new()
            {
                Key = "marginRange",
                Label = "Margin Range",
                Type = "percentageRange"
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "grossPremium",
                Label = "Gross Premium",
                Format = "currency"
            },
            new()
            {
                Key = "insurerCost",
                Label = "Insurer Cost",
                Format = "currency"
            },
            new()
            {
                Key = "cscCommission",
                Label = "CSC Commission",
                Format = "currency"
            },
            new()
            {
                Key = "netAdminProfit",
                Label = "Net Admin Profit",
                Format = "currency"
            },
            new()
            {
                Key = "averageMargin",
                Label = "Average Margin",
                Format = "percentage"
            }
                ]
            };
        }

        private static EnquiryReportDefinition CreateDocumentPendingReport()
        {
            return new EnquiryReportDefinition
            {
                Type = EnquiryReportType.DocumentPending,
                Name = "Document Pending Report",
                Description = "Identify enquiries with pending or missing documents.",

                Filters =
                [
                    new()
            {
                Key = "enquiryDateRange",
                Label = "Enquiry Date",
                Type = "dateRange"
            },
            new()
            {
                Key = "employee",
                Label = "Employee",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "missingDocument",
                Label = "Missing Document",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "agency",
                Label = "Agency",
                Type = "multiSelect",
                Multiple = true
            },
            new()
            {
                Key = "ageing",
                Label = "Ageing",
                Type = "multiSelect",
                Multiple = true
            }
                ],

                Metrics =
                [
                    new()
            {
                Key = "totalEnquiries",
                Label = "Total Enquiries",
                Format = "number"
            },
            new()
            {
                Key = "totalMissingDocuments",
                Label = "Total Missing Documents",
                Format = "number"
            },
            new()
            {
                Key = "averageDaysPending",
                Label = "Average Days Pending",
                Format = "number"
            }
                ]
            };
        }
    }
}
