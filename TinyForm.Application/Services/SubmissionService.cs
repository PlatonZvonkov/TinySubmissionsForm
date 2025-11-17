using TinyForm.Core.Interfaces;
using System.Text.Json;
using TinyForm.Core.Entities;

namespace TinyForm.Application.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _repo;

    public SubmissionService(ISubmissionRepository repo)
    {
        _repo = repo;
    }

    public async Task<Submission> CreateAsync(string formType, JsonDocument jsonPayload)
    { 
        var payloadJson = jsonPayload.RootElement.GetRawText();

        var submission = new Submission()
        {
            FormType = formType,
            Payload = payloadJson,
            SubmittedAt = DateTime.UtcNow,
        };

        return await _repo.AddAsync(submission);
    }

    public async Task<Submission?> GetByIdAsync(Guid id)
    {
        var result =  await _repo.GetByIdAsync(id);
        return result;
    }

    public async Task<IEnumerable<Submission>> QueryAsync(string? formType, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        int skip = (page - 1) * pageSize;
        var result = await _repo.QueryAsync(formType, search, skip, pageSize, cancellationToken);
        return result;
    }
}

