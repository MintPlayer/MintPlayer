using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MintPlayer.Data.Entities.Interfaces;

namespace MintPlayer.Data.Entities.Blog;

internal class BlogPost : ISoftDelete
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	public string Title { get; set; }
	public string Headline { get; set; }
	public string Body { get; set; }

	public User UserInsert { get; set; }
	public User UserUpdate { get; set; }
	public User UserDelete { get; set; }

	public DateTime DateInsert { get; set; }
	public DateTime? DateUpdate { get; set; }
	public DateTime? DateDelete { get; set; }
}
