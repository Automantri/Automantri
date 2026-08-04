using Automantri.Application.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/admin/import")]
[Authorize(Roles = "Admin")]
public sealed class AdminImportController(ICatalogImportService importService) : ControllerBase
{
    [HttpPost("preview")]
    [RequestSizeLimit(20_000_000)]
    [ProducesResponseType<CatalogImportPreviewResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogImportPreviewResultDto>> Preview(
        IFormFile file,
        [FromForm] string brand = "Hyundai",
        [FromForm] int year = 2024,
        [FromForm] bool syncDeletes = true,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Upload an .xlsx file." });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only .xlsx files are supported." });
        }

        await using var stream = file.OpenReadStream();
        var result = await importService.PreviewAsync(
            stream,
            file.FileName,
            new CatalogImportPreviewRequest(brand, year, syncDeletes),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("commit")]
    [ProducesResponseType<CatalogImportCommitResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogImportCommitResultDto>> Commit(
        [FromBody] CatalogImportCommitRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Rows is null || request.Rows.Count == 0)
        {
            return BadRequest(new { message = "No rows to import." });
        }

        var result = await importService.CommitAsync(request, cancellationToken);
        return Ok(result);
    }
}
