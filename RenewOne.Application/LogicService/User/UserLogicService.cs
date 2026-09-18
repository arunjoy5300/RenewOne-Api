using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Employees;
using RenewOne.Application.DTOs.Request.User;
using RenewOne.Application.DTOs.Response.Employees;
using RenewOne.Application.DTOs.Response.User;
using RenewOne.Application.Filters.User;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.User;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Users
{
    public class UserLogicService : IUserLogicService
    {
        #region DI
        private readonly IUserRepository _userRepository;
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserTaskAllocationRepository _userTaskAllocationRepository;
        private readonly IUserModulePermissionRepository _userModulePermissionRepository;
        private readonly ICurrentUserService _currentUser;
        #endregion

        #region Constructor
        public UserLogicService(IUserRepository userRepository, 
            IEnquiryRepository enquiryRepository, 
            IUnitOfWork unitOfWork, 
            IUserTaskAllocationRepository userTaskAllocationRepository,
            IUserModulePermissionRepository userModulePermissionRepository,
            ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _enquiryRepository = enquiryRepository;
            _unitOfWork = unitOfWork;
            _userTaskAllocationRepository = userTaskAllocationRepository;
            _userModulePermissionRepository = userModulePermissionRepository;
            _currentUser = currentUser;
        }
        #endregion

        #region Admin Staff Users

        #region Create Admin Internal(Staff) User
        public async Task<Result<bool>> CreateInternalUserAsync(CreateInternalUserRequest request)
        {
            var validUserTypes = new[] { "SuperAdmin", "Supervisor", "AdminEmployee" };

            if (!validUserTypes.Contains(request.UserType))
                return Result<bool>.Failure("Invalid user type.", 400);

            if (await _userRepository.GetByUserNameAsync(request.UserName) != null)
                return Result<bool>.Conflict("Username already exists.");

            if (await _userRepository.GetByLoginIdentifierAsync(request.Email) != null)
                return Result<bool>.Conflict("Email already exists.");

            if (request.ModulePermissions == null || !request.ModulePermissions.Any())
                return Result<bool>.Failure("Please assign at least one module.", 400);

            if (request.WorkAllocations == null || !request.WorkAllocations.Any())
                return Result<bool>.Failure("Please assign at least one work allocation.", 400);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    AccountId = null,
                    UserName = request.UserName,
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserType = request.UserType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddUserAsync(user);

                // Save Work Allocations
                foreach (var allocation in request.WorkAllocations)
                {
                    if (allocation.VehicleTypeIds == null || !allocation.VehicleTypeIds.Any())
                    {
                        await _userTaskAllocationRepository.AddAsync(new UserTaskAllocation
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            InsuranceTypeId = allocation.InsuranceTypeId,
                            VehicleTypeId = null,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });

                        continue;
                    }

                    foreach (var vehicleTypeId in allocation.VehicleTypeIds.Distinct())
                    {
                        await _userTaskAllocationRepository.AddAsync(new UserTaskAllocation
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            InsuranceTypeId = allocation.InsuranceTypeId,
                            VehicleTypeId = vehicleTypeId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Save Module Permissions
                foreach (var module in request.ModulePermissions.Distinct())
                {
                    await _userModulePermissionRepository.AddAsync(new UserModulePermission
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ModuleName = module,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(
                    true,
                    $"{request.UserType} account created successfully.",
                    201);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return Result<bool>.Failure(
                    $"An error occurred while creating the user. {ex.Message}",
                    500);
            }
        }
        #endregion

        #region View All Admin Internal(Staff) User
        public async Task<Result<PagedResponse<EmployeeListResponse>>> GetAdminEmployee(EmployeeListRequest request)
        {
            var query = _userRepository.GetAllUsers().Where(x => x.AccountId == null && x.UserType != "SuperAdmin");

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(u => u.UserName.Contains(request.Search) || u.FullName.Contains(request.Search) || u.Email.Contains(request.Search));
            }
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.IsActive == request.Status.Value);
            }
            var totalRecords = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new EmployeeListResponse
                {
                    EmployeeId = x.Id,
                    EmployeeName = x.FullName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Role = x.UserType,
                    Status = x.IsActive,
                    ActiveTask = _enquiryRepository.GetQueryable().Count(e => e.AssignedEmployeeId == x.Id && e.Status != EnquiryStatus.PolicyIssued.ToString() && e.Status != EnquiryStatus.Cancelled.ToString()),
                    CompletedTask = _enquiryRepository.GetQueryable().Count(e => e.AssignedEmployeeId == x.Id && e.Status == EnquiryStatus.PolicyIssued.ToString()),
                    CreatedOn = x.CreatedAt
                })
                .ToListAsync();

            var response = new PagedResponse<EmployeeListResponse>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize),
                Items = data
            };
            return Result<PagedResponse<EmployeeListResponse>>.Success(
                response,
                "Employees fetched successfully");
        }
        #endregion

        #region Toggle Admin Internal(Staff) User Status (Activate/Deactivate)
        public async Task<Result<bool>> ToggleAdminEmployeeStatusAsync(Guid employeeId)
        {
            try
            {
                var employee = await _userRepository.GetByIdAsync(employeeId);

                if (employee == null || employee.AccountId != null)
                    return Result<bool>.NotFound("Employee not found.");

                employee.IsActive = !employee.IsActive;

                await _userRepository.UpdateUserAsync(employee);

                return Result<bool>.Success(
                    true,
                    employee.IsActive
                        ? "Employee activated successfully."
                        : "Employee deactivated successfully.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"An error occurred while toggling employee status: {ex.Message}", 500);
            }
        }
        #endregion

        #region Admin Staff Dashboard Overview
        public async Task<Result<AdminStaffDashboardResponse>> GetAdminStaffDashboardOverviewAsync()
        {
            var enquiries = _enquiryRepository.GetQueryable();
            var totalEmployees = await _userRepository.GetAllUsers().CountAsync(x => x.AccountId == null && x.UserType != "SuperAdmin");
            var activeEmployees = await _userRepository.GetAllUsers().CountAsync(x => x.AccountId == null && x.UserType != "SuperAdmin" && x.IsActive);
            var inactiveEmployees = totalEmployees - activeEmployees;
            var totalActiveTask = await enquiries.CountAsync(x =>
                x.AssignedEmployeeId != null &&
                x.Status != EnquiryStatus.PolicyIssued.ToString() &&
                x.Status != EnquiryStatus.Cancelled.ToString());

            var totalCompletedTask = await enquiries.CountAsync(x =>
                x.AssignedEmployeeId != null &&
                x.Status == EnquiryStatus.PolicyIssued.ToString());

            var response = new AdminStaffDashboardResponse
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                TotalActiveTask = totalActiveTask,
                TotalCompletedTask = totalCompletedTask
            };
            return Result<AdminStaffDashboardResponse>.Success(response, "Admin staff dashboard overview fetched successfully.");
        }
        #endregion

        #region View Admin Internal(Staff) User Rights
        public async Task<Result<EmployeeRightsResponse>> GetAdminEmployeeRightsAsync(Guid employeeId)
        {
            var employee = await _userRepository.GetByIdAsync(employeeId);

            if (employee == null || employee.AccountId != null)
                return Result<EmployeeRightsResponse>.NotFound("Employee not found.");

            var modulePermissions = await _userModulePermissionRepository.GetByUserIdAsync(employeeId);
            var workAllocations = await _userTaskAllocationRepository.GetQueryable().Where(x => x.UserId == employeeId).ToListAsync();

            var response = new EmployeeRightsResponse
            {
                UserId = employee.Id,
                FullName = employee.FullName,
                UserType = employee.UserType,
                ModulePermissions = modulePermissions
                .Select(x => x.ModuleName)
                .ToList(),

                        WorkAllocations = workAllocations
                .GroupBy(x => x.InsuranceTypeId)
                .Select(g => new WorkAllocationResponse
                {
                    InsuranceTypeId = g.Key,
                    VehicleTypeIds = g
                        .Where(x => x.VehicleTypeId.HasValue)
                        .Select(x => x.VehicleTypeId!.Value)
                        .ToList()
                })
                .ToList()
            };
            return Result<EmployeeRightsResponse>.Success(response, "Employee rights fetched successfully.");
        }
        #endregion

        #region Update Admin Internal(Staff) User Rights
        public async Task<Result<bool>> UpdateAdminEmployeeRightsAsync(UpdateEmployeeRightsRequest request)
        {
            var employee = await _userRepository.GetByIdAsync(request.UserId);

            if (employee == null || employee.AccountId != null)
                return Result<bool>.NotFound("Employee not found.");

            if (request.ModulePermissions == null || !request.ModulePermissions.Any())
                return Result<bool>.Failure("Please assign at least one module.", 400);

            if (request.WorkAllocations == null || !request.WorkAllocations.Any())
                return Result<bool>.Failure("Please assign at least one work allocation.", 400);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Delete existing module permissions
                var existingModulePermissions = await _userModulePermissionRepository.GetByUserIdAsync(request.UserId);

                if (existingModulePermissions.Any())
                {
                    await _userModulePermissionRepository.DeleteRangeAsync(existingModulePermissions);
                }

                // Delete existing work allocations
                var existingWorkAllocations = await _userTaskAllocationRepository
                    .GetQueryable()
                    .Where(x => x.UserId == request.UserId)
                    .ToListAsync();

                if (existingWorkAllocations.Any())
                {
                    await _userTaskAllocationRepository.DeleteRangeAsync(existingWorkAllocations);
                }

                // Add module permissions
                foreach (var module in request.ModulePermissions.Distinct())
                {
                    await _userModulePermissionRepository.AddAsync(new UserModulePermission
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId,
                        ModuleName = module,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                // Add work allocations
                foreach (var allocation in request.WorkAllocations)
                {
                    // Non-vehicle insurance
                    if (allocation.VehicleTypeIds == null || !allocation.VehicleTypeIds.Any())
                    {
                        await _userTaskAllocationRepository.AddAsync(new UserTaskAllocation
                        {
                            Id = Guid.NewGuid(),
                            UserId = request.UserId,
                            InsuranceTypeId = allocation.InsuranceTypeId,
                            VehicleTypeId = null,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });

                        continue;
                    }

                    // Vehicle insurance
                    foreach (var vehicleTypeId in allocation.VehicleTypeIds.Distinct())
                    {
                        await _userTaskAllocationRepository.AddAsync(new UserTaskAllocation
                        {
                            Id = Guid.NewGuid(),
                            UserId = request.UserId,
                            InsuranceTypeId = allocation.InsuranceTypeId,
                            VehicleTypeId = vehicleTypeId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, "Employee rights updated successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return Result<bool>.Failure(
                    $"An error occurred while updating employee rights: {ex.Message}",
                    500);
            }
        }
        #endregion

        #region Update Admin Internal(Staff) User Details
        public async Task<Result<bool>> UpdateAdminEmployeeAsync(UpdateAdminEmployeeRequest request)
        {
            var employee = await _userRepository.GetByIdAsync(request.UserId);

            if (employee == null || employee.AccountId != null)
                return Result<bool>.NotFound("Employee not found.");

            // Check Email
            var emailExists = await _userRepository.GetByLoginIdentifierAsync(request.Email);

            if (emailExists != null && emailExists.Id != request.UserId)
                return Result<bool>.Conflict("Email already exists.");

            // Check Phone Number
            var phoneExists = await _userRepository.GetAllUsers().Where(x => x.PhoneNumber == request.PhoneNumber).FirstOrDefaultAsync();

            if (phoneExists != null && phoneExists.Id != request.UserId)
                return Result<bool>.Conflict("Phone number already exists.");

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                employee.FullName = request.FullName.Trim();
                employee.Email = request.Email.Trim();
                employee.PhoneNumber = request.PhoneNumber.Trim();
                employee.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateUserAsync(employee);

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, "Employee updated successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return Result<bool>.Failure(
                    $"An error occurred while updating the employee: {ex.Message}",
                    500);
            }
        }
        #endregion

        #endregion

        #region Agent Staff Users

        #region Create Agent Internal(Staff) User
        public async Task<Result<bool>> CreateStaffAsync(CreateStaffRequest request, Guid accountId)
        {
            if (await _userRepository.GetByUserNameAsync(request.UserName) != null)
                return Result<bool>.Conflict("Username already exists");

            if (await _userRepository.GetByLoginIdentifierAsync(request.Email) != null)
                return Result<bool>.Conflict("Email already exists");

            var newStaff = new User
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserType = "Staff",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddUserAsync(newStaff);
            return Result<bool>.Success(true, "Staff Created Successfully", 201);
        }
        #endregion

        #region Get My Staff List
        public async Task<Result<List<StaffResponse>>> GetMyStaffAsync(Guid accountId)
        {
            var staffList = await _userRepository.GetAgentStaffByAccountAsync(accountId);

            var response = staffList.Select(s => new StaffResponse
            {
                Id = s.Id,
                UserName = s.UserName,
                FullName = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList();

            return Result<List<StaffResponse>>.Success(response);
        }
        #endregion

        #region List Staff with Filters
        public async Task<Result<PagedResponse<StaffResponse>>> ListStaffWithFiltersAsync(Guid accountId, StaffFilterRequest request)
        {
            try
            {
                var query = _userRepository.GetAllUsers()
                    .Where(x => x.AccountId == accountId);

                // Search
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var search = request.Search.Trim().ToLower();

                    query = query.Where(x =>
                        x.UserName.ToLower().Contains(search) ||
                        x.FullName.ToLower().Contains(search) ||
                        x.Email.ToLower().Contains(search) ||
                        x.PhoneNumber.Contains(search));
                }

                // Status Filter
                if (request.Status.HasValue)
                {
                    query = query.Where(x => x.IsActive == request.Status.Value);
                }

                var totalRecords = await query.CountAsync();

                var staffList = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var response = staffList.Select(s => new StaffResponse
                {
                    Id = s.Id,
                    UserName = s.UserName,
                    FullName = s.FullName,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToList();

                return Result<PagedResponse<StaffResponse>>.Success(
                    new PagedResponse<StaffResponse>
                    {
                        Items = response,
                        TotalRecords = totalRecords,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize)
                    },
                    "Staff fetched successfully");
            }
            catch (Exception ex)
            {
                return Result<PagedResponse<StaffResponse>>.Failure(
                    $"Error fetching staff: {ex.Message}",
                    500);
            }
        }
        #endregion

        #region Toggle Staff Status (Activate/Deactivate)
        public async Task<Result<bool>> ToggleStaffStatusAsync(Guid staffId, Guid accountId)
        {
            var staff = await _userRepository.GetByIdAsync(staffId);

            if (staff == null || staff.AccountId != accountId)
                return Result<bool>.NotFound("Staff not exists");

            staff.IsActive = !staff.IsActive;
            await _userRepository.UpdateUserAsync(staff);

            string statusMessage = staff.IsActive ? "Staff account activated." : "Staff account deactivated.";

            return Result<bool>.Success(true, statusMessage);
        }
        #endregion

        #endregion
    }
}
