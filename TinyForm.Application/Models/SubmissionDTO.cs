using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using TinyForm.Application.Validations;

namespace TinyForm.Application.Models;

public class SubmissionDTO
{
    [Required]
    [NotEmptyString(ErrorMessage = "Invalid password")]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression("^[A-Za-z0-9_-]+$",
        ErrorMessage = "FormType must be alphanumeric, underscores or hyphens.")]
    public string FormType { get; set; } = "unknown";
    
    [JsonPropertyName("payload")]
    [Required]
    public JsonDocument Payload { get; set; }
}
