using System.Text.Json;
using TinyForm.Core.Entities;
using TinyForm.Core.Interfaces;

namespace TinyForm.WebAPI.Proxy
{
    public class SubmissionServiceLoggingDecorator : ISubmissionService
    {
        private readonly ISubmissionService _inner;
        private readonly ILogger<SubmissionServiceLoggingDecorator> _logger;

        public SubmissionServiceLoggingDecorator(ISubmissionService inner, ILogger<SubmissionServiceLoggingDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<Submission> CreateAsync(string formType, JsonDocument jsonPayload)
        {
            if (string.IsNullOrWhiteSpace(formType))
            {
                _logger.LogWarning("Rejecting submission: formType is empty.");
                throw new ArgumentNullException($"Invalid form type {nameof(formType)}");
            }
            _logger.LogInformation($"Creating new submission of type {formType}");
            
            return await _inner.CreateAsync(formType, jsonPayload);
        }

        public Task<IEnumerable<Submission>> QueryAsync(string? formType, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page <= 0 || pageSize <= 0)
            {
                 _logger.LogWarning("Rejecting query: page size cannot be negative number.");
                throw new ArgumentNullException($"page size cannot be negative number");
            }
            _logger.LogInformation($"Query submissions: formType={formType}, search={search}");

            return _inner.QueryAsync(formType, search, page, pageSize, cancellationToken);
        }

        public Task<Submission?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation($"Getting submission with ID={id}");

            return _inner.GetByIdAsync(id);
        }
    }
}
