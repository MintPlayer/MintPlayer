using System;
using System.ServiceProcess;

namespace MintPlayerCrawler.Service
{
    public class MintPlayerCrawlerService : ServiceBase
    {
        public MintPlayerCrawlerService()
        {

        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            Console.WriteLine("Started MintPlayer crawler service");
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        protected override void OnStop()
        {
            base.OnStop();
            Console.WriteLine("Stopped MintPlayer crawler service");
        }
    }
}
