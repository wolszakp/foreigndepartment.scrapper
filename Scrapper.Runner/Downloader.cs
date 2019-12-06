using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Scrapper.Common;

namespace Scrapper.Runner
{
    public class Downloader
    {
        private readonly IScrapper _scrapper;
        private readonly string _outputPath;
        private readonly string _entryUrl;
        private readonly string _articlesCacheFilePath; 
        private readonly string _downloadUrlsFilePath;
        
        private static HttpClient _http = new HttpClient();

        public Downloader(IScrapper scrapper, string outputPath, string entryUrl)
        {
            _scrapper = scrapper;
            _outputPath = outputPath;
            _entryUrl = entryUrl;
            _articlesCacheFilePath = Path.Combine(_outputPath, "articles.txt");
            _downloadUrlsFilePath = Path.Combine(_outputPath, "download.txt");
        }

        public async Task Run()
        {
            EnsureDirectoryExists(_outputPath);

            var articlesUrls = await GetArticleUrls(_entryUrl);
            var downloadUrls = await GetDownloadUrls(articlesUrls);

            foreach(var item in downloadUrls)
            {
                await DownloadFile(item);
            }
        }

        private void EnsureDirectoryExists(string outputPath)
        {
            if(!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
        }

        private async Task<IEnumerable<string>> GetArticleUrls(string entryUrl)
        {
            if(File.Exists(_articlesCacheFilePath)) 
            {
                Console.WriteLine($"Article urls cache exists: {_articlesCacheFilePath}");
                return await File.ReadAllLinesAsync(_articlesCacheFilePath);
            }

            var articlesUrls = await _scrapper.GetAllArticleUrls(entryUrl);
            await File.WriteAllLinesAsync(_articlesCacheFilePath, articlesUrls);
            return articlesUrls;
        }

        private async Task<IEnumerable<string>> GetDownloadUrls(IEnumerable<string> articlesUrls)
        {
            if(File.Exists(_downloadUrlsFilePath)) 
            {
                System.Console.WriteLine($"Download list cache exists: {_downloadUrlsFilePath}");
                return await File.ReadAllLinesAsync(_downloadUrlsFilePath);
            }

            var downloadUrls = await _scrapper.GetDownloadUrlsFromArticlePages(articlesUrls);

            await File.WriteAllLinesAsync(_downloadUrlsFilePath, downloadUrls);
            return downloadUrls;
        }

        private async Task DownloadFile(string downloadUrl) 
        {
            var fileName = downloadUrl.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = Path.Combine(_outputPath, fileName);
            var fileInfo = new FileInfo(filePath);
            
            if(fileInfo.Exists)
            {
                Console.WriteLine($"File {fileName} already exists ({fileInfo.Length.ToSize(ByteSizeExtensions.SizeUnits.MB)}MB).");
                return;
            }

            var response = await _http.GetAsync(new Uri(downloadUrl));
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            {
                await response.Content.CopyToAsync(fs);
                Console.WriteLine($"Saved {fileName} ({fs.Length.ToSize(ByteSizeExtensions.SizeUnits.MB)}MB)");
            }
        }
    }
}