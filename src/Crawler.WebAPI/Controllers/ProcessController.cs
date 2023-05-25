using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using Crawler.WebAPI.DTOs;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Crawler.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessController : Controller
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessController> _logger;

    public ProcessController(IPublishEndpoint publishEndpoint, ILogger<ProcessController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Start(UrlDto dto)
    {
        var id = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting Start method with URL: {Url}", dto.Url);
        await _publishEndpoint.Publish(new RequestedUrl
        {
            Id = id,
            Url = dto.Url
        });

        _logger.LogInformation("Published RequestedUrl message successfully");
        await using (AppDbContext context = new())
        {
            await context.Links.AddAsync(new Link
                { Id = id, SourceId = Guid.Empty.ToString(), Url = dto.Url, Status = Status.Requested });
            await context.SaveChangesAsync();
            _logger.LogInformation("New Job added to the repository successfully");
        }

        _logger.LogInformation("Returning Ok from Start method");
        return Ok();
    }
}