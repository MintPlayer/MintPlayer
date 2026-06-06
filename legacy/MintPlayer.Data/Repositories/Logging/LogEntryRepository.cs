using System.Threading.Tasks;

namespace MintPlayer.Data.Repositories.Logging
{
    public interface ILogEntryRepository
    {
        Task Log(string text);
    }

    internal class LogEntryRepository : ILogEntryRepository
    {
        private readonly MintPlayerContext mintPlayerContext;
        public LogEntryRepository(MintPlayerContext mintPlayerContext)
        {
            this.mintPlayerContext = mintPlayerContext;
        }

        public async Task Log(string text)
        {
            var entity = new Entities.Logging.LogEntry
            {
                Text = text
            };

            await mintPlayerContext.LogEntries.AddAsync(entity);
            await mintPlayerContext.SaveChangesAsync();
        }
    }
}
