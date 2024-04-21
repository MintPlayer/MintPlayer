using MintPlayer.Fetcher.Integration.Enums;

namespace MintPlayer.Fetcher.Integration.Dtos;

public class SubjectWithCertainty<TSubject> where TSubject : MintPlayer.Dtos.Dtos.Subject
{
	public TSubject Subject { get; set; }
	public ECertainty Certainty { get; set; }
}
