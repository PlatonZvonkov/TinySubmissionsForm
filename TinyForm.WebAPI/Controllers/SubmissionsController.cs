using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using TinyForm.Application.Models;
using TinyForm.Core.Interfaces;

namespace TinyForm.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    public SubmissionsController(ISubmissionService service)
    {
        _submissionService = service;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SubmissionDTO body)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
               
        if (body.Payload == null)
        {
            return BadRequest("Payload must contain valid JSON.");
        }
       
        if (body.Payload.ToString().Length > 100_000)  // Prevent extremely large JSON payloads ( 100 KB limit example)
        {
            return BadRequest("Payload exceeds allowed size.");
        }

        var created = await _submissionService.CreateAsync(body.FormType, body.Payload);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? formType = null, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return BadRequest("Page must be >= 1.");
        }

        if (pageSize < 1 || pageSize > 200)
        {
            return BadRequest("PageSize must be between 1 and 200.");
        }
       
        if (search?.Length > 200)
        {
            return BadRequest("Search term too long.");
        }
        var results = await _submissionService.QueryAsync(formType, search, page, pageSize, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _submissionService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }
}
