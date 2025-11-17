using Moq;
using TinyForm.Application.Services;
using TinyForm.Core.Entities;
using TinyForm.Core.Interfaces;

namespace TinyFor.EFSubmissionRepositoryTests
{
    public class SubmissionServiceTests
    {
        private readonly Mock<ISubmissionRepository> _mockRepo;
        private readonly ISubmissionService _service;

        public SubmissionServiceTests()
        {
            _mockRepo = new Mock<ISubmissionRepository>();
            _service = new SubmissionService(_mockRepo.Object);
        }

       
        [Theory]
        [InlineData(1, 10, 0)] // Page 1, Size 10, should skip 0
        [InlineData(3, 20, 40)] // Page 3, Size 20, should skip 40
        public async Task QueryAsync_ShouldCalculateSkipCorrectly_AndMapToDto(int page, int pageSize, int expectedSkip)
        {
            // Arrange
            var formType = "Order";
            var submissions = new List<Submission>
        {
            new() { Id = Guid.NewGuid(), FormType = "Order", SubmittedAt = DateTime.Now },
        };

            _mockRepo.Setup(r => r.QueryAsync(formType, null, expectedSkip, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(submissions);

            // Act
            var results = await _service.QueryAsync(formType, null, page, pageSize);

            // Assert            
            _mockRepo.Verify(r => r.QueryAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                expectedSkip,
                pageSize,
                It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify mapping to DTO
            var resultList = results.ToList();
            Assert.Single(resultList);
        }
    }
}
