using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace Selenium.Scrapper
{
    public class SeleniumScrapper
    {
        private readonly string _browserPath;
        private readonly ChromeOptions _options;

        public SeleniumScrapper()
        {
            _browserPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _options = new ChromeOptions();
            _options.AddArguments("--window-position=0,0",  "--window-size=1,1");
            // _options.AddArguments("--start-maximized");
        }

        public IEnumerable<string> GetDownloadUrlsFromArticlePages(IEnumerable<string> articleUrls)
        {
            var downloadUrls = new List<string>();
            foreach(var item in articleUrls) 
            {
                var downloadUrl = GetDownloadUrlFromArticlePage(item);
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

        public string GetDownloadUrlFromArticlePage(string baseUrl)
        {
            //var url = @"https://www.dzialzagraniczny.pl/2019/11/jak-z-trauma-radza-sobie-tysiace-bosniaczek-zgwalconych-podczas-wojny-domowej-dzial-zagraniczny-podcast021/";

            using(var driver = new ChromeDriver(_browserPath, _options))
            {
                driver.Navigate().GoToUrl(baseUrl);
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.00));
                // check for complete load
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                // var playerDiv = driver.FindElementsByCssSelector("div.row.player");
                // var commentsDiv = driver.FindElementById("comments");
                // var moreArticelsDiv = driver.FindElementsByCssSelector("div.td-more-articles-box");
                // var scrollAction = new Actions(driver);
                // scrollAction.MoveToElement(commentsDiv);
                // scrollAction.Perform();

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
                
                return downloadUrl;
            }
        }

        public IEnumerable<string> ReadAllPodcastLinks(string url)
        {
            var urls = new List<string>();
            var visitedUrls = new List<string>();
            var nextUrl = url;
            using(var driver = new ChromeDriver(_browserPath, _options))
            do
                {
                    visitedUrls.Add(nextUrl);
                    driver.Navigate().GoToUrl(nextUrl);
                    urls.AddRange(ReadPageLinks(driver));

                    nextUrl = ReadNextPageLink(driver);
                } while (!visitedUrls.Contains(nextUrl));

            return urls;
        }

        private string ReadNextPageLink(ChromeDriver driver)
        {
            var nextPageSelector = "div.page-nav.td-pb-padding-side > a";
            var nextPage = driver.FindElementsByCssSelector(nextPageSelector);
            var nextUrl = nextPage?.Last().GetAttribute("href");
            return nextUrl;
        }

        private IEnumerable<string> ReadPageLinks(ChromeDriver driver)
        {
            var hrefsSelector = "div.td_module_5.td_module_wrap.td-animation-stack > div.td-item-details.td-category-small > h3.entry-title.td-module-title > a";
            var hrefs = driver.FindElementsByCssSelector(hrefsSelector);

            var urls = hrefs.Select(m => m.GetAttribute("href"));
            return urls;
        }
    }
}
