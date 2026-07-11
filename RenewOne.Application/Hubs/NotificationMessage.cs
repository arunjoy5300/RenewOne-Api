namespace RenewOne.Application.Hubs
{
    namespace RenewOne.Application.Hubs
    {
        public class NotificationMessage
        {
            public string Event { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;

            public string Message { get; set; } = string.Empty;

            public Guid? ReferenceId { get; set; }

            public string? Status { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public object? Data { get; set; }
        }
    }
}
