namespace RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote
{
    public interface IEnquiryQuoteRepository
    {
        Task AddEnquiryQuoteAsync(Domain.Entities.EnquiryQuote enquiryQuote);
        Task<bool> SaveChangesAsync();
    }
}
