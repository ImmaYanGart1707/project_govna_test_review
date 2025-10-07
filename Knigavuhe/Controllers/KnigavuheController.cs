using Knigavuhe.Services;
using Microsoft.AspNetCore.Mvc;

namespace Knigavuhe.Controllers;

[ApiController]
[Route("[controller]")]
public class KnigavuheController(
    KnigavuheService knigavuheService) : ControllerBase
{
    [HttpGet("WriteAuthorsLinksToCsv")]
    public async Task<IActionResult> WriteAuthorsLinksToCsv()
    {
        await knigavuheService.WriteAuthorsLinksToCsv();
        return Ok();
    }
    
    [HttpGet("WriteAuthorsToCsv")]
    public async Task<IActionResult> WriteAuthorsToCsv()
    {
        await knigavuheService.WriteAuthorsToCsv();
        return Ok();
    }
    
    [HttpGet("WriteTrackToCsv")]
    public async Task<IActionResult> WriteTrackToCsv()
    {
        await knigavuheService.WriteTrackToCsv();
        return Ok();
    }
    
    [HttpGet("DownloadMp3")]
    public async Task<IActionResult> DownloadMp3()
    {
        await knigavuheService.DownloadMp3();
        return Ok();
    }
}