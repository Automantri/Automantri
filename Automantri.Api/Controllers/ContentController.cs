using Automantri.Application.Frontend;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/content")]
public sealed class ContentController(IContentService contentService) : ControllerBase
{
    [HttpGet("testimonials")]
    [ProducesResponseType<IReadOnlyCollection<TestimonialDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<TestimonialDto>> GetTestimonials() =>
        Ok(contentService.GetTestimonials());

    [HttpGet("activity")]
    [ProducesResponseType<IReadOnlyCollection<ActivityItemDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<ActivityItemDto>> GetActivity() =>
        Ok(contentService.GetRecentActivity());

    [HttpGet("stats")]
    [ProducesResponseType<PlatformStatsDto>(StatusCodes.Status200OK)]
    public ActionResult<PlatformStatsDto> GetStats() =>
        Ok(contentService.GetPlatformStats());

    [HttpGet("journey")]
    [ProducesResponseType<JourneySectionDto>(StatusCodes.Status200OK)]
    public ActionResult<JourneySectionDto> GetJourney() =>
        Ok(contentService.GetJourney());
}
