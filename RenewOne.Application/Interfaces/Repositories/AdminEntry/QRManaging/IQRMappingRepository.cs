using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging
{
    public interface IQRMappingRepository
    {
        IQueryable<QRMapping> GetQueryable();
        Task<QRMapping?> GetByIdAsync(Guid id);
        Task AddAsync(QRMapping mapping);
        void Update(QRMapping mapping);
    }
}
