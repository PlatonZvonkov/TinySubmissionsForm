using TinyForm.Core.Entities;

namespace TinyForm.Core.Interfaces;
public interface ISubmissionRepository
{
    Task InitializeAsync();
    Task<Submission> AddAsync(Submission submission);
    Task<IEnumerable<Submission>> GetAllAsync();
    Task<IEnumerable<Submission>> QueryAsync(string? formType = null, string? search = null, int skip = 0, int take = 100, CancellationToken cancellationToken = default);
    Task<Submission?> GetByIdAsync(Guid id);
}

