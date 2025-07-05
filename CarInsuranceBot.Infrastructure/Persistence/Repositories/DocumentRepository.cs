using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _db;
    public DocumentRepository(ApplicationDbContext db) => _db = db;

    public void Add(Document doc) => _db.Documents.Add(doc);
    public Task<Document?> GetAsync(Guid id, CancellationToken ct) =>
        _db.Documents.FindAsync(new object?[] { id }, ct).AsTask();
}