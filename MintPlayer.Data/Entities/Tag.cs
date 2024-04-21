using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MintPlayer.Data.Entities;

internal class Tag
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	public string Description { get; set; }
	public TagCategory Category { get; set; }
	public Tag Parent { get; set; }
	public List<Tag> Children { get; set; }
	public List<SubjectTag> Subjects { get; set; }

	public User UserInsert { get; set; }
	public User UserUpdate { get; set; }
	public User UserDelete { get; set; }

	public DateTime DateInsert { get; set; }
	public DateTime? DateUpdate { get; set; }
	public DateTime? DateDelete { get; set; }
}
