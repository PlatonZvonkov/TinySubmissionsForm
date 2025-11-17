using System.Text.Json;

namespace TinyForm.Core.Entities;

public record Submission
{
    public Guid Id { get; set; } =  Guid.NewGuid();
    public string FormType { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string Payload { get; set; } = "{}";

}
