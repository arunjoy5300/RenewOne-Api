namespace RenewOne.Application.Interfaces.Repositories.Vendor
{
    public interface IVendorRepository
    {
        Task AddAsync(Domain.Entities.Vendor entity, CancellationToken ct);
        void Update(Domain.Entities.Vendor entity, CancellationToken ct = default);
        IQueryable<Domain.Entities.Vendor> GetQueryable();
    }
}
