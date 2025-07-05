namespace CarInsuranceBot.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        // Repositories (only one for now)
        IUserRepository Users { get; }
        IDocumentRepository Documents { get; }
        IExtractedFieldRepository ExtractedFields { get; }   // new
        IPolicyRepository Policies { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
