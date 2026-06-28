using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry.PricingQoute
{
    public class EnquiryQuoteRepository : IEnquiryQuoteRepository
    {
        private readonly AppDbContext _dbContext;
        public EnquiryQuoteRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddEnquiryQuoteAsync(EnquiryQuote enquiryQuote)
        {
            await _dbContext.EnquiryQuotes.AddAsync(enquiryQuote);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }
        public IQueryable<EnquiryQuote> GetEnquiryQuoteQuery()
        {
            return _dbContext.EnquiryQuotes;
        }
    }
}
