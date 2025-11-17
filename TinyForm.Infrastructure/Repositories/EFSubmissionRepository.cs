using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TinyForm.Core.Entities;
using TinyForm.Core.Interfaces;

namespace TinyForm.Infrastructure.Repositories;
   public class EFSubmissionRepository : ISubmissionRepository
{
    private readonly SubmissionDbContext _db;
    private readonly ILogger<EFSubmissionRepository> _logger;

    public EFSubmissionRepository(SubmissionDbContext db, ILogger<EFSubmissionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // For EF Core In-Memory, no explicit loading  needed
        _logger.LogInformation("EF InMemory DataBase Initialised");
        await Task.CompletedTask;
    }

    public async Task<Submission> AddAsync(Submission submission)
    {
         await InitializeAsync();
        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();
        return submission;
    }

    public async Task<Submission?> GetByIdAsync(Guid id)
    {
         await InitializeAsync();
        var result = await _db.Submissions.FirstOrDefaultAsync(s => s.Id == id);
        return result;
    }

    public async Task<IEnumerable<Submission>> GetAllAsync()
    {
         await InitializeAsync();
        var result = await _db.Submissions.ToListAsync();
        return result;
    }

    public async Task<IEnumerable<Submission>> QueryAsync(string? formType = null, string? search = null, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
         await InitializeAsync();
        IQueryable<Submission> query = _db.Submissions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(formType))
        {
            query = query.Where(x => x.FormType.Equals(formType, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Payload.ToLower().Contains(search.ToLower())
                                    || x.FormType.ToLower().Contains(search.ToLower()));
        }

        var results = await query
            .OrderByDescending(x => x.SubmittedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return results;
    }
}
