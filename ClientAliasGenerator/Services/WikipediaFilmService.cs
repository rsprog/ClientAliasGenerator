using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TenantAliasGenerator.Services
{
    public class WikipediaFilmService : IFilmService
    {
        private readonly HttpClient httpClient;
        private readonly Regex regex;
        private const string url = "https://en.wikipedia.org/wiki/List_of_films:_";
        private const string pattern = @"<li><i><a href=""\/wiki\/.+"" title="".+"">(?<title>.+)<\/a><\/i> \((?<year>\d+).*\)<\/li>";

        public WikipediaFilmService() : this(new HttpClientHandler())
        {
        }

        public WikipediaFilmService(HttpMessageHandler httpMessageHandler)
        {
            httpClient = new HttpClient(httpMessageHandler);
            regex = new Regex(pattern, RegexOptions.Compiled);
        }

        public async Task<FilmInfo[]> GetFilmsThatStartWithLetterAsync(char letter) =>
            ScrapeForFilmInfos(await GetPageContents(url + char.ToUpper(letter)));

        private async Task<string> GetPageContents(string url)
        {
            var response = await httpClient.GetAsync(url);
            return response.StatusCode != HttpStatusCode.OK ? "" : await response.Content.ReadAsStringAsync();
        }

        private FilmInfo[] ScrapeForFilmInfos(string markup) =>
            regex.Matches(markup).Select(m => new FilmInfo { Title = m.Groups["title"].Value, Year = m.Groups["year"].Value }).ToArray();
    }
}
