using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface ILogEntryService
	{
		Task Log(string text);
	}
}
