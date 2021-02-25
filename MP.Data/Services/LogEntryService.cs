using MintPlayer.Data.Repositories.Logging;
using System.Threading.Tasks;

namespace MintPlayer.Data.Services
{
    public interface ILogEntryService
    {
        Task Log(string text);
    }

    internal class LogEntryService : ILogEntryService
    {
        private readonly ILogEntryRepository logEntryRepository;
        public LogEntryService(ILogEntryRepository logEntryRepository)
        {
            this.logEntryRepository = logEntryRepository;
        }

        public async Task Log(string text)
        {
            await logEntryRepository.Log(text);
        }
    }
}
