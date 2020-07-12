using MintPlayer.Fetcher.Dtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Muzikum
{
    internal class MuzikumFetcher : Fetcher
    {
        private readonly IRequestSender requestSender;
        public MuzikumFetcher(IRequestSender requestSender)
        {
            this.requestSender = requestSender;
        }

        public override IEnumerable<Regex> UrlRegex
        {
            get
            {
                return new[]
                {
                    new Regex(@"https\:\/\/(www\.){0,1}muzikum.eu\/.+")
                };
            }
        }

        public override async Task<Subject> Fetch(string url, bool trimTrash)
        {
            var html = await requestSender.SendRequest(url);
            throw new NotImplementedException();
        }

        #region Private methods
        #region ParseSong

        #endregion
        #endregion
    }
}
