using Microsoft.AspNetCore.Mvc;
using MintPlayer.Web.PublicSite;

namespace MintPlayer.Web.Controllers;

/// <summary>
/// Anonymous read of the public song view, for client-side navigation to <c>/song/{id}</c>. The first
/// (server-rendered) load gets the same <see cref="PublicSongDto"/> via prerendering + TransferState.
/// </summary>
[ApiController]
[Route("api/public/song")]
public class PublicSongController : ControllerBase
{
    private readonly PublicSongReader reader;

    public PublicSongController(PublicSongReader reader)
    {
        this.reader = reader;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PublicSongDto>> Get(string id, CancellationToken cancellationToken)
        => await reader.GetAsync(id, cancellationToken) is { } song ? song : NotFound();
}
