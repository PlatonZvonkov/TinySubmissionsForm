using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using TinyForm.Core.Entities;
using TinyForm.Core.Interfaces;

namespace TinyForm.Infrastructure.Repositories
{
    public class FileSubmissionRepository : ISubmissionRepository
    {
        private readonly string _filePath;
        private readonly ILogger<FileSubmissionRepository> _logger;
        private readonly ConcurrentDictionary<Guid, Submission> _store = new();
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly SemaphoreSlim _startupLock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        private bool _loaded;

        public FileSubmissionRepository(string filePath, ILogger<FileSubmissionRepository> logger)
        {
            _filePath = filePath;
            _logger = logger;
        }
        public async Task InitializeAsync()
        {
            if (_loaded) return; 
            await _startupLock.WaitAsync();
            try
            {                
                await LoadFromFileAsync();
                _loaded = true;
            }
            finally
            {
                _startupLock.Release();
            }
        }

        public async Task<Submission> AddAsync(Submission submission)
        {
            await InitializeAsync();
            _store[submission.Id] = submission;            
            await PersistToFileAsync();            
            return submission;
        }

        public async Task<IEnumerable<Submission>> GetAllAsync()
        {
            await InitializeAsync();
            return _store.Values;
        }

        public async Task<IEnumerable<Submission>> QueryAsync(string? formType = null, string? search = null, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
        {
           await InitializeAsync();
           IEnumerable<Submission> query = _store.Values;
            if (!string.IsNullOrWhiteSpace(formType))
            {
                query = query.Where(x => x.FormType.Equals(formType, StringComparison.OrdinalIgnoreCase));

            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Payload.ToLower().Contains(search.ToLower())
                || x.FormType.ToLower().Contains(search.ToLower())
                );                    
            }
            query = query.OrderByDescending(x => x.SubmittedAt).Skip(skip).Take(take);

            return query;
        }

        public async Task<Submission?> GetByIdAsync(Guid id)
        {
            await InitializeAsync();
            _store.TryGetValue(id, out var result);
            return result;
        }

        /**
            <summary> 
                create empty file if there is none
           </summary>
        **/
        private async Task LoadFromFileAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    await File.WriteAllTextAsync(_filePath, "[]");
                    return;
                }
                var text = await File.ReadAllTextAsync(_filePath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    await File.WriteAllTextAsync(_filePath, "[]");
                    return;
                }
                var items = JsonSerializer.Deserialize<List<Submission>>(text);
                if (items != null)
                {
                    foreach (var item in items)
                        _store[item.Id] = item;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load submissions from file.");
                throw;
            }
        }

        /**
           <summary> Ensure only one writer to file at a time.
            And Persist so callers know it's saved
           </summary>
        **/
        private async Task PersistToFileAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                var snapshot = _store.Values
                    .OrderBy(item => item.SubmittedAt)
                    .ToList();

                var directory = Path.GetDirectoryName(_filePath)!;
                var tempPath = Path.Combine(directory, $"{Path.GetFileName(_filePath)}.tmp");
                                
                await using (var tempStream = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await JsonSerializer.SerializeAsync(tempStream, snapshot, _jsonOptions);
                    await tempStream.FlushAsync();
                }
                                
                if (File.Exists(_filePath))
                {
                    File.Replace(tempPath, _filePath, null, true);
                }
                else
                {
                    File.Move(tempPath, _filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist submissions to file.");
                throw;
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
}
