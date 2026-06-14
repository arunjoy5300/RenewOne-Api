using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Documents
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetAllDocumentsByEnquiryId(Guid enquiryId);
        Task AddAsync(Document document);
        Task<bool> SaveChangesAsync();
    }
}
