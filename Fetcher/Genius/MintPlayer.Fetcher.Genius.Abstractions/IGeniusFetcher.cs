using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MintPlayer.Fetcher.Abstractions.Dtos;

namespace MintPlayer.Fetcher.Genius.Abstractions
{
	public interface IGeniusFetcher
	{
		Task<Subject> Fetch(string url, bool trimTrash);
	}
}
