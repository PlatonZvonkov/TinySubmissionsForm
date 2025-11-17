using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TinyForm.Core.Entities;
using TinyForm.Infrastructure.Repositories;

namespace TinyFor.Tests
{
    public class FileSubmissionRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly FileSubmissionRepository _repository;
        private readonly Mock<ILogger<FileSubmissionRepository>> _mockLogger;

        public FileSubmissionRepositoryTests()
        {
            // Create a temporary, unique file for testing
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_submissions_{Guid.NewGuid()}.json");

            _mockLogger = new Mock<ILogger<FileSubmissionRepository>>();
            _repository = new FileSubmissionRepository(_testFilePath, _mockLogger.Object);
        }

        [Fact]
        public async Task InitializeAsync_ShouldCreateFile_IfNotFound()
        {
            // Arrange: File does not exist initially
            File.Delete(_testFilePath);

            // Act
            await _repository.InitializeAsync();

            // Assert
            Assert.True(File.Exists(_testFilePath));
            Assert.Equal("[]", await File.ReadAllTextAsync(_testFilePath));
        }

        [Fact]
        public async Task InitializeAsync_ShouldLoadData_FromFile()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var initialData = new List<Submission>
        {
            new() { Id = submissionId, FormType = "LoadTest", Payload = "{\"test\":\"loaded\"}" }
        };
            await File.WriteAllTextAsync(_testFilePath, JsonSerializer.Serialize(initialData));

            // Act
            await _repository.InitializeAsync();
            var results = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.Single(results);
            Assert.Equal(submissionId, results.First().Id);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistToFile_AndOverwriteExistingFile()
        {
            // Arrange
            await _repository.InitializeAsync(); // Ensure the file exists
            var submission1 = new Submission { FormType = "A", Payload = "{\"v\":1}" };

            // Act
            await _repository.AddAsync(submission1);

            // Assert
            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Contains(submission1.Id.ToString(), fileContent);

            // Add a second submission to ensure it overwrites correctly
            var submission2 = new Submission { FormType = "B", Payload = "{\"v\":2}" };
            await _repository.AddAsync(submission2);

            // Verify both are present in the final file content
            var finalContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Contains(submission1.Id.ToString(), finalContent);
            Assert.Contains(submission2.Id.ToString(), finalContent);
        }

        [Fact]
        public async Task QueryAsync_ShouldFilterAndPaginate_InMemory()
        {
            // Arrange
            await _repository.InitializeAsync();
            var sub1 = new Submission { FormType = "Contact", SubmittedAt = DateTime.UtcNow.AddHours(-1) };
            var sub2 = new Submission { FormType = "Order", SubmittedAt = DateTime.UtcNow };
            var sub3 = new Submission { FormType = "Contact", SubmittedAt = DateTime.UtcNow.AddHours(-2), Payload = "{\"msg\":\"special\"}" };

            await _repository.AddAsync(sub1);
            await _repository.AddAsync(sub2);
            await _repository.AddAsync(sub3);

            // Act 1: Filter by FormType "Contact"
            var results1 = (await _repository.QueryAsync(formType: "contact")).ToList();
            Assert.Equal(2, results1.Count());

            // Act 2: Search for "special"
            var results2 = (await _repository.QueryAsync(search: "special")).ToList();
            Assert.Single(results2);
            Assert.Equal(sub3.Id, results2.First().Id);

            // Act 3: Pagination (Order is sub2, sub1, sub3) -> Skip 1, Take 1 should return sub1
            var results3 = (await _repository.QueryAsync(skip: 1, take: 1)).ToList();
            Assert.Single(results3);
            Assert.Equal(sub1.Id, results3.First().Id);
        }

        public void Dispose()
        {
            // Clean up the temporary file after each test
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
    }
}
