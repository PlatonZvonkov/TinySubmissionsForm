using System.Text.Json;
using TinyForm.Core.Entities;

namespace TinyForm.Core.Interfaces;

public interface ISubmissionService
{
    Task<Submission> CreateAsync(string formType, JsonDocument jsonPayload);
    Task<IEnumerable<Submission>> QueryAsync(string? formType, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Submission?> GetByIdAsync(Guid id);
}
