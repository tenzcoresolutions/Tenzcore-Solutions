using ApiWeb.Application.DTOs;
using ApiWeb.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IConfiguration _configuration;

    public MessagesController(IMessageService messageService, IConfiguration configuration)
    {
        _messageService = messageService;
        _configuration = configuration;
    }

    [HttpPost]
    [EnableRateLimiting("submit-policy")]
    public async Task<ActionResult<MessageDto>> Submit([FromBody] SubmitMessageRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Honey))
        {
            return BadRequest("Invalid request");
        }
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 300)
        {
            return BadRequest("Text is required and must be <= 300 characters.");
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var result = await _messageService.CreateAsync(request.Text, ip, ua, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MessageDto>>> Get([FromQuery] string? sort = "recent", [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new FeedQuery
        {
            SortBy = string.Equals(sort, "top", StringComparison.OrdinalIgnoreCase) ? SortBy.Top : SortBy.Recent,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100)
        };
        var result = await _messageService.GetFeedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MessageDto>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var message = await _messageService.GetByIdAsync(id, cancellationToken);
        return message is null ? NotFound() : Ok(message);
    }

    [HttpPost("{id:guid}/vote")]
    [EnableRateLimiting("vote-policy")]
    public async Task<ActionResult<object>> Vote([FromRoute] Guid id, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        if (request.Value != -1 && request.Value != 1)
        {
            return BadRequest("Vote value must be -1 or 1.");
        }
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        var score = await _messageService.VoteAsync(id, request.Value, ip, ua, cancellationToken);
        if (score == 0)
        {
            // Could be not found; double-check
            var exists = await _messageService.GetByIdAsync(id, cancellationToken);
            if (exists is null) return NotFound();
        }
        return Ok(new { score });
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var requiredKey = _configuration["Admin:ApiKey"];
        var provided = Request.Headers["X-Admin-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(requiredKey) || !string.Equals(requiredKey, provided))
        {
            return Forbid();
        }
        var ok = await _messageService.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}