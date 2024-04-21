namespace MintPlayer.Data.Entities;

internal class SubjectTag
{
	public SubjectTag()
	{
	}
	public SubjectTag(Subject subject, Tag tag) : this()
	{
		Subject = subject;
		SubjectId = subject.Id;
		Tag = tag;
		TagId = tag.Id;
	}

	public int SubjectId { get; set; }
	public Subject Subject { get; set; }

	public int TagId { get; set; }
	public Tag Tag { get; set; }
}
