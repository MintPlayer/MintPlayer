using MintPlayer.Dtos.Dtos;
using Microsoft.AspNetCore.Identity;

namespace MintPlayer.Data.Repositories;

internal interface IMediumRepository
{
	Task StoreMedia(Subject subject, IEnumerable<Medium> media);
}
internal class MediumRepository : IMediumRepository
{
	private readonly MintPlayerContext mintplayer_context;
	private readonly IHttpContextAccessor http_context;
	private readonly UserManager<Entities.User> user_manager;
	public MediumRepository(
		MintPlayerContext mintplayer_context,
		IHttpContextAccessor http_context,
		UserManager<Entities.User> user_manager)
	{
		this.mintplayer_context = mintplayer_context;
		this.http_context = http_context;
		this.user_manager = user_manager;
	}

	public async Task StoreMedia(Subject subject, IEnumerable<Medium> media)
	{
		var user = await user_manager.GetUserAsync(http_context.HttpContext.User);
		var isAdmin = await user_manager.IsInRoleAsync(user, "Administrator");

		var query = mintplayer_context.Media.Where(m => m.Subject.Id == subject.Id);
		if (!isAdmin)
		{
			query = query.Where(m => m.Type.Visible);
		}

		// Remove the old media
		foreach (var medium in query)
			mintplayer_context.Media.Remove(medium);

		// Get entity from database
		var entity_subject = await mintplayer_context.Subjects.FindAsync(subject.Id);
		if (media != null)
		{
			foreach (var medium in media)
			{
				var entity_medium = new Entities.Medium
				{
					Subject = entity_subject,
					Type = await mintplayer_context.MediumTypes.FindAsync(medium.Type.Id),
					Value = medium.Value
				};
				await mintplayer_context.AddAsync(entity_medium);
			}
		}
	}
}
