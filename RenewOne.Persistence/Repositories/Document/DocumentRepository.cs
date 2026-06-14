using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Documents;


namespace RenewOne.Persistence.Repositories.Document
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _dbContext;
        public DocumentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Domain.Entities.Document>> GetAllDocumentsByEnquiryId(Guid enquiryId)
        {
            return await _dbContext.Documents
                .Where(d => d.EnquiryId == enquiryId)
                .ToListAsync();
        }
        public async Task AddAsync(Domain.Entities.Document document)
        {
            await _dbContext.Documents.AddAsync(document);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
