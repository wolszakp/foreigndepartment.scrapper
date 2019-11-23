using System;

namespace ForeignCountry.Console
{
    class Program
    {
        private static HttpClient HttpClient = new HttpClient();
        private static string OutputPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static string PageLinksFileName = Path.Combine(OutputPath, "pageLinks.txt");
        private static string Mp3LinksFileName = Path.Combine(OutputPath, "mp3Links.txt");
        private static string OutputMp3Path = Path.Combine(OutputPath, "out");

        static void Main(string[] args)
        {
            var mainUrl = @"https://www.dzialzagraniczny.pl/";
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
                return await File.ReadAllLinesAsync(Mp3LinksFileName);
            }

            var seleniumScrapper = new Selenium.Scrapper.SeleniumScrapper();
            var downloadUrls = seleniumScrapper.GetDownloadUrlsFromArticlePages(podcastPagesUrls);            
            await File.WriteAllLinesAsync(Mp3LinksFileName, downloadUrls);
            return downloadUrls;
        }

        private static async Task<IEnumerable<string>> ReadArticleUrls(string url)
        {
            if(File.Exists(PageLinksFileName)) 
            {
                return await File.ReadAllLinesAsync(PageLinksFileName);
            }

            var seleniumScrapper = new Selenium.Scrapper.SeleniumScrapper();
            var podcastPagesUrls = seleniumScrapper.ReadAllPodcastLinks(url);            
            await File.WriteAllLinesAsync(PageLinksFileName, podcastPagesUrls);
            return podcastPagesUrls;
        }

        static async Task DownloadMp3File(string url) 
        {
            var fileName = url.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();

            var uri = new Uri(url);
            var response = await HttpClient.GetAsync(uri);
            if(!Directory.Exists(OutputMp3Path))
            {
                Directory.CreateDirectory(OutputMp3Path);
            }
            
            using (var fs = new FileStream(Path.Combine(OutputMp3Path, fileName), FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Saved {fileName}");
                Console.ResetColor();
            }
        }
    }
}
