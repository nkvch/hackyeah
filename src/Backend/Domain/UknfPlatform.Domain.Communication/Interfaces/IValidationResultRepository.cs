using UknfPlatform.Domain.Communication.Entities;

namespace UknfPlatform.Domain.Communication.Interfaces;

public interface IValidationResultRepository
{
    Task<ValidationResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ValidationResult?> GetByReportIdAsync(Guid reportId, CancellationToken cancellationToken = default);
    Task<ValidationResult?> GetByValidationIdAsync(string validationId, CancellationToken cancellationToken = default);
    Task AddAsync(ValidationResult validationResult, CancellationToken cancellationToken = default);
    Task UpdateAsync(ValidationResult validationResult, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


