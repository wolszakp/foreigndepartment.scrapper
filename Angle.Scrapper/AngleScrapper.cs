using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace Angle.Scrapper
{
    public class AngleScrapper
    {
        private readonly IBrowsingContext _context;

        public AngleScrapper()
        {
            var configuration = Configuration.Default
                .WithDefaultLoader()
                .WithConsoleLogger(ctx => new ConsoleLogger());
            _context = BrowsingContext.New(configuration);
        }

        public async Task DownloadMp3FromLink(string url)
        {
            var document = await _context.OpenAsync(url);
            var mp3Selector = "play-player";
            var link = document.GetElementById(mp3Selector);
        }

        public async Task<IEnumerable<string>> ReadAllPodcastLinks(string url)
        {
            var urls = new List<string>();
            var visitedUrls = new List<string>();
            var nextUrl = url;
            do
            {
                visitedUrls.Add(nextUrl);
                var document = await _context.OpenAsync(nextUrl);
                urls.AddRange(ReadPageLinks(document));

                var nextPageSelector = "div.page-nav.td-pb-padding-side > a";
                var nextPage = document.QuerySelectorAll(nextPageSelector);
                nextUrl = nextPage?.Last().GetAttribute("href");
            } while (!visitedUrls.Contains(nextUrl));

            return urls;
        }

        public IEnumerable<string> ReadPageLinks(IDocument document)
        {
            var hrefsSelector = "div.td_module_5.td_module_wrap.td-animation-stack > div.td-item-details.td-category-small > h3.entry-title.td-module-title > a";
            var hrefs = document.QuerySelectorAll(hrefsSelector);

            var urls = hrefs.Select(m => m.GetAttribute("href"));
            return urls;
        }
    }
}