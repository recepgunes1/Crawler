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

    public ProcessController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> Start(UrlDto dto)
    {
        await _publishEndpoint.Publish(new RequestedUrl()
        {
            Url = dto.Url
        });
        return Ok();
    }
    
}