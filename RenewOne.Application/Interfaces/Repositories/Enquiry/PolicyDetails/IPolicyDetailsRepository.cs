namespace RenewOne.Application.Interfaces.Repositories.Enquiry.PolicyDetails
{
    public interface IPolicyDetailsRepository
    {
        IQueryable<Domain.Entities.PolicyDetails> GetQueryable();

        Task<Domain.Entities.PolicyDetails?> GetByIdAsync(
            Guid id,
            CancellationToken ct);

        Task AddAsync(
            Domain.Entities.PolicyDetails entity,
            CancellationToken ct);

        void Update(Domain.Entities.PolicyDetails entity);
    }
}
