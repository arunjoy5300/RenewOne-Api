using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.DTOs.Response.User
{
    public class StaffResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StaffListResponse
    {
        public PagedResponse<StaffResponse> Staff { get; set; } = null!;

        public int TotalStaff { get; set; }

        public int ActiveStaff { get; set; }

        public int InactiveStaff { get; set; }
    }
}
