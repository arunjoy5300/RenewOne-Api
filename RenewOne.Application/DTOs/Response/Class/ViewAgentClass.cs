namespace RenewOne.Application.DTOs.Response.Class
{
    public class ViewAgentClass
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public decimal CommSharePercentage { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
