using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Scrapper.Common;

namespace ForeignDailyNews.Scrapper
{
    public class Scrapper : IScrapper
    {
        private readonly string _browserPath;
        private readonly ChromeOptions _options;

        public Scrapper()
        {
            _browserPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _options = new ChromeOptions();
            _options.AddArguments("--window-position=0,0",  "--window-size=1,1");
        }

        public async Task<IEnumerable<string>> GetDownloadUrlsFromArticlePages(IEnumerable<string> articleUrls)
        {
            var downloadUrls = new List<string>();
            foreach(var item in articleUrls) 
            {
                var downloadUrl = await GetDownloadUrlFromArticlePage(item);
                if( downloadUrl != null) 
                {
                    downloadUrls.Add(downloadUrl);
                    continue;
                }
                
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Download url doesn't exist for: {item}");
                Console.ResetColor();
            }
            return downloadUrls;
        }

        public Task<string> GetDownloadUrlFromArticlePage(string baseUrl)
        {
            using(var driver = new ChromeDriver(_browserPath, _options))
            {
                driver.Navigate().GoToUrl(baseUrl);
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.00));
                // check for complete load
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                var iframes = driver.FindElementsByCssSelector("iframe");
                var src = iframes.FirstOrDefault()?.GetAttribute("src");
                
                if(src == null) 
                {
                    return null;
                }
                
                driver.Navigate().GoToUrl(src);
                
                object playlistObject = null;
                try
                {
                    playlistObject = driver.ExecuteScript("return playList");
                } catch (OpenQA.Selenium.WebDriverException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Exception occured for {baseUrl}.\n{ex.Message}\n{ex.StackTrace}");
                    Console.ResetColor();
                }
                
                if(playlistObject == null) 
                {
                    return null;
                }

                var playList = JArray.FromObject(playlistObject);
                var downloadUrl = playList.FirstOrDefault()?.Value<string>("download_link");
                
                return Task.FromResult(downloadUrl);
            }
        }

        public Task<IEnumerable<string>> GetAllArticleUrls(string entryUrl)
        {
            var urls = new HashSet<string>();
            var visitedUrls = new List<string>();
            var nextUrl = entryUrl;
            using(var driver = new ChromeDriver(_browserPath, _options))
            do
                {
                    visitedUrls.Add(nextUrl);
                    driver.Navigate().GoToUrl(nextUrl);
                    urls.AddRange(ReadArticleUrls(driver));

                    nextUrl = ReadNextPageUrl(driver);
                } while (!visitedUrls.Contains(nextUrl));

            return Task.FromResult(urls.AsEnumerable());
        }

        private string ReadNextPageUrl(ChromeDriver driver)
        {
            var nextPageSelector = "div.page-nav.td-pb-padding-side > a";
            var nextPage = driver.FindElementsByCssSelector(nextPageSelector);
            var nextUrl = nextPage?.Last().GetAttribute("href");
            return nextUrl;
        }

        private IEnumerable<string> ReadArticleUrls(ChromeDriver driver)
        {
            var hrefsSelector = "div.td_module_5.td_module_wrap.td-animation-stack > div.td-item-details.td-category-small > h3.entry-title.td-module-title > a";
            var hrefs = driver.FindElementsByCssSelector(hrefsSelector);

            var urls = hrefs.Select(m => m.GetAttribute("href"));
            return urls;
        }
    }
}
