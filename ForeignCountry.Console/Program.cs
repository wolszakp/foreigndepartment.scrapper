using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Howtosavemoney.Scrapper;

namespace ForeignCountry.Console
{
    class Program
    {
        // outDirectory
        // verbose
        // jakoszczędzać + dziennik zagraniczny
        // Force refresh

        private static HttpClient HttpClient = new HttpClient();
        private static string OutputPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "wnop");
        private static string PageLinksFileName = Path.Combine(OutputPath, "pageLinks.txt");
        private static string Mp3LinksFileName = Path.Combine(OutputPath, "mp3Links.txt");
        private static string OutputMp3Path = Path.Combine(OutputPath, "out");

        static void Main(string[] args)
        {
            if(!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }
            // var mainUrl = @"https://www.dzialzagraniczny.pl/";
            var mainUrl = @"https://jakoszczedzacpieniadze.pl/podcast";

            MainAsync(mainUrl).GetAwaiter().GetResult();            
        }

        static async Task MainAsync(string url)
        {
            var articlesUrls = await ReadArticleUrls(url);
            var downloadUrls = await ReadDownloadUrls(articlesUrls);

            foreach(var item in downloadUrls)
            {
                await DownloadMp3File(item);
            }
        }

        private static async Task<IEnumerable<string>> ReadDownloadUrls(IEnumerable<string> podcastPagesUrls)
        {
            if(File.Exists(Mp3LinksFileName)) 
            {
                System.Console.WriteLine($"Download list cache exists: {Mp3LinksFileName}");
                return await File.ReadAllLinesAsync(Mp3LinksFileName);
            }

            //var seleniumScrapper = new Selenium.Scrapper.SeleniumScrapper();
            //var downloadUrls = seleniumScrapper.GetDownloadUrlsFromArticlePages(podcastPagesUrls);            
            var scrapper = new Scrapper();
            var downloadUrls = await scrapper.GetDownloadUrlsFromArticlePages(podcastPagesUrls);

            await File.WriteAllLinesAsync(Mp3LinksFileName, downloadUrls);
            return downloadUrls;
        }

        private static async Task<IEnumerable<string>> ReadArticleUrls(string url)
        {
            if(File.Exists(PageLinksFileName)) 
            {
                System.Console.WriteLine($"Article urls cache exists: {PageLinksFileName}");
                return await File.ReadAllLinesAsync(PageLinksFileName);
            }

            // var seleniumScrapper = new Selenium.Scrapper.SeleniumScrapper();
            // var podcastPagesUrls = seleniumScrapper.ReadAllPodcastLinks(url);
            var scrapper = new Scrapper();
            var podcastPagesUrls = await scrapper.ReadAllPodcastLinks(url);
            await File.WriteAllLinesAsync(PageLinksFileName, podcastPagesUrls);
            return podcastPagesUrls;
        }

        static async Task DownloadMp3File(string url) 
        {
            var fileName = url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = Path.Combine(OutputMp3Path, fileName);
            if(File.Exists(filePath))
            {
                System.Console.WriteLine($"File ${fileName} already exists.");
                return;
            }

            var uri = new Uri(url);
            var response = await HttpClient.GetAsync(uri);
            if(!Directory.Exists(OutputMp3Path))
            {
                Directory.CreateDirectory(OutputMp3Path);
            }

            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"Saved {fileName}");
                System.Console.ResetColor();
            }
        }
    }
}
