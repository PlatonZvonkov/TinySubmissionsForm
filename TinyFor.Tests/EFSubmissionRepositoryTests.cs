using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TinyForm.Core.Entities;
using TinyForm.Infrastructure;
using TinyForm.Infrastructure.Repositories;

namespace TinyFor.EFSubmissionRepositoryTests
{
    public class EFSubmissionRepositoryTests : IDisposable
    {
        private readonly SubmissionDbContext _context;
        private readonly EFSubmissionRepository _repository;

        public EFSubmissionRepositoryTests()
        {
            // Use a unique database name for each test class
            var options = new DbContextOptionsBuilder<SubmissionDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SubmissionDbContext(options);

            // Mock logger for the repository
            var mockLogger = new Mock<ILogger<EFSubmissionRepository>>();
            _repository = new EFSubmissionRepository(_context, mockLogger.Object);

            // Seed initial data
            SeedData().Wait();
        }

        private async Task SeedData()
        {
            _context.Submissions.AddRange(new List<Submission>
        {
            new() { Id = new Guid("10000000-0000-0000-0000-000000000001"), FormType = "Contact", SubmittedAt = DateTime.UtcNow.AddHours(-2), Payload = "{\"email\":\"test@email.com\",\"message\":\"Inquiry 1\"}" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000002"), FormType = "Order", SubmittedAt = DateTime.UtcNow.AddHours(-1), Payload = "{\"product\":\"Laptop\",\"quantity\":1}" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000003"), FormType = "Contact", SubmittedAt = DateTime.UtcNow, Payload = "{\"email\":\"other@test.com\",\"message\":\"Support Request\"}" },
            new() { Id = new Guid("10000000-0000-0000-0000-000000000004"), FormType = "Feedback", SubmittedAt = DateTime.UtcNow.AddHours(-3), Payload = "{\"rating\":5,\"comment\":\"Great service\"}" },
        });
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddAsync_ShouldPersistSubmission_AndReturnIt()
        {
            // Arrange
            var newSubmission = new Submission { FormType = "NewType", Payload = "{\"data\":\"value\"}" };

            // Act
            var addedSubmission = await _repository.AddAsync(newSubmission);

            // Assert
            Assert.NotEqual(Guid.Empty, addedSubmission.Id);
            var retrieved = await _context.Submissions.FindAsync(addedSubmission.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("NewType", retrieved.FormType);
        }

        [Fact]
        public async Task QueryAsync_ShouldFilterByFormType_CaseInsensitive()
        {
            // Act
            var results = (await _repository.QueryAsync(formType: "contact")).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.True(results.All(s => s.FormType == "Contact"));
        }

        [Fact]
        public async Task QueryAsync_ShouldSearchInPayloadBlob_AndFormType()
        {
            // Act 1: Search in PayloadBlob (email or product)
            var results1 = (await _repository.QueryAsync(search: "Laptop")).ToList();
            Assert.Single(results1);
            Assert.Equal("Order", results1.First().FormType);

            // Act 2: Search in FormType
            var results2 = (await _repository.QueryAsync(search: "feedback")).ToList();
            Assert.Single(results2);
            Assert.Equal("Feedback", results2.First().FormType);
        }

        [Fact]
        public async Task QueryAsync_ShouldHandlePagination_AndOrderDescendingBySubmittedAt()
        {
            // Act
            var results = (await _repository.QueryAsync(skip: 1, take: 2)).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            // The submissions are ordered by SubmittedAt descending: 3, 2, 1, 4
            // Skipping 1 means we get the 2nd and 3rd submissions: 2 and 1
            Assert.Equal(new Guid("10000000-0000-0000-0000-000000000002"), results.First().Id); // Order
            Assert.Equal(new Guid("10000000-0000-0000-0000-000000000001"), results.Last().Id);  // Contact
        }


        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
