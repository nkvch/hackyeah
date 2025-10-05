using Microsoft.EntityFrameworkCore;
using UknfPlatform.Domain.Communication.Entities;
using UknfPlatform.Domain.Communication.Interfaces;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Infrastructure.Persistence.Repositories;

public class ValidationResultRepository : IValidationResultRepository
{
    private readonly ApplicationDbContext _context;

    public ValidationResultRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ValidationResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ValidationResults
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<ValidationResult?> GetByReportIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        return await _context.ValidationResults
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.ReportId == reportId, cancellationToken);
    }

    public async Task<ValidationResult?> GetByValidationIdAsync(string validationId, CancellationToken cancellationToken = default)
    {
        return await _context.ValidationResults
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UniqueValidationId == validationId, cancellationToken);
    }

    public async Task AddAsync(ValidationResult validationResult, CancellationToken cancellationToken = default)
    {
        await _context.ValidationResults.AddAsync(validationResult, cancellationToken);
    }

    public Task UpdateAsync(ValidationResult validationResult, CancellationToken cancellationToken = default)
    {
        _context.ValidationResults.Update(validationResult);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}


