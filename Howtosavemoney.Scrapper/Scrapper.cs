using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace Howtosavemoney.Scrapper
{
    public class Scrapper
    {
        private readonly IBrowsingContext _context;

        public Scrapper()
        {
            var configuration = Configuration.Default
                .WithDefaultLoader();
            _context = BrowsingContext.New(configuration);
        }

        public async Task<IEnumerable<string>> GetDownloadUrlsFromArticlePages(IEnumerable<string> articleUrls)
        {
            var downloadUrls = new HashSet<string>();
            foreach(var item in articleUrls) 
            {
                var downloadUrl = await GetDownloadUrlFromArticlePage(item);
                if( downloadUrl != null) 
                {
                    downloadUrls.Add(downloadUrl);
                    continue;
                
                }
                
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Download url doesn't exist for: {item}");
                Console.ResetColor();
            }
            return downloadUrls;
        }

        public async Task<string> GetDownloadUrlFromArticlePage(string url)
        {
            var document = await _context.OpenAsync(url);
            var mp3Selector = "p.powerpress_links.powerpress_links_mp3 > a.powerpress_link_d";
            var link = document.QuerySelector(mp3Selector);
            return link?.GetAttribute("href");
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

                var nextPageSelector = "p.pagination > a";
                var nextPage = document.QuerySelectorAll(nextPageSelector);
                nextUrl = nextPage?.SkipWhile(n => n.ClassName != "current").Skip(1).FirstOrDefault()?.GetAttribute("href");
            } while (!string.IsNullOrEmpty(nextUrl) && !visitedUrls.Contains(nextUrl));

            return urls.Distinct();
        }

        public IEnumerable<string> ReadPageLinks(IDocument document)
        {
            var hrefsSelector = "div.teaser > h2.entry-title > a";
            var hrefs = document.QuerySelectorAll(hrefsSelector);

            var urls = hrefs.Select(m => m.GetAttribute("href"));
            return urls;
        }
    }
}
