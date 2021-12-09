using System;
using System.Collections.Generic;
using System.Text;

namespace MintPlayer.Fetcher.Integration.Dtos
{
	public class FetchResult
	{
	}

	public class FetchResult<TSubject> : FetchResult where TSubject : FetchedSubject
	{
		public TSubject FetchedSubject { get; set; }
		public IEnumerable<SubjectWithCertainty<MintPlayer.Dtos.Dtos.Subject>> Candidates { get; set; }
	}
}
