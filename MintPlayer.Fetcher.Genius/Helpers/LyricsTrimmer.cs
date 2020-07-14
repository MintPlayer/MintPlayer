using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MintPlayer.Fetcher.Genius.Helpers
{
    internal interface ILyricsTrimmer
    {
        Task<string> Trim(string html, bool trimTrash);
    }
    internal class LyricsTrimmer : ILyricsTrimmer
    {
        public Task<string> Trim(string html, bool trimTrash)
        {
            var pRegex = new Regex(@"(?<=\<p\>).*?(?=\<\/p\>)", RegexOptions.Singleline | RegexOptions.Multiline);
            var pMatch = pRegex.Match(html);
            if (!pMatch.Success) throw new Exception("No P tag found");

            var stripARegex = new Regex(@"\<a.*?\>|\<\/a\>|\<i\>|\<\/i\>", RegexOptions.Singleline | RegexOptions.Multiline);
            var stripped = stripARegex.Replace(pMatch.Value, "");
            var whitespaces_stripped = stripped.Replace("\r", "").Replace("\n", "").Replace("<br>", Environment.NewLine);

            if (trimTrash)
            {
                var stripBracketsRegex = new Regex(@"\[.*?\]\r\n", RegexOptions.Singleline | RegexOptions.Multiline);
                var brackets_stripped = stripBracketsRegex.Replace(whitespaces_stripped, "");
                return Task.FromResult(brackets_stripped);
            }
            else
            {
                return Task.FromResult(whitespaces_stripped);
            }
        }
    }
}
