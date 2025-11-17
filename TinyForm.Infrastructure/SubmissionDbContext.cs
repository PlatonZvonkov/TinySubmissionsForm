using Microsoft.EntityFrameworkCore;
using TinyForm.Core.Entities;

namespace TinyForm.Infrastructure;

public class SubmissionDbContext  : DbContext
{
    public SubmissionDbContext(DbContextOptions<SubmissionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Submission> Submissions => Set<Submission>();
}
