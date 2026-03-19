using Log.Application.DTOs;
using Log.Domain.Entities;
using Log.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Log.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly LogDbContext _logDbContext;

    public LogsController(LogDbContext logDbContext)
    {
        _logDbContext = logDbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLogRequestDto request, CancellationToken cancellationToken)
    {
        var logEntry = new LogEntry
        {
            Id = Guid.NewGuid(),
            ServiceName = request.ServiceName,
            Level = request.Level,
            Message = request.Message,
            Exception = request.Exception,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _logDbContext.Logs.AddAsync(logEntry, cancellationToken);
        await _logDbContext.SaveChangesAsync(cancellationToken);

        return Ok("Log kaydedildi.");
    }

    [Authorize(Policy = "LogsReadPolicy")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var logs = await _logDbContext.Logs
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("log service is running");
    }
}
