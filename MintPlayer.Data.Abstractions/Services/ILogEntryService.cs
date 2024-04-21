namespace MintPlayer.Data.Abstractions.Services;

public interface ILogEntryService
{
	Task Log(string text);
}
