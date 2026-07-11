using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging
{
    public interface IQRMasterRepository
    {
        IQueryable<QRMaster> GetQueryable();

        Task<QRMaster?> GetByIdAsync(Guid id);

        Task AddAsync(QRMaster qrMaster);

        void Update(QRMaster qrMaster);
    }
}
