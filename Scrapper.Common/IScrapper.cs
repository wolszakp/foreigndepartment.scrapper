using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scrapper.Common
{
    public interface IScrapper
    {
        Task<IEnumerable<string>> GetDownloadUrlsFromArticlePages(IEnumerable<string> articleUrls);

        Task<string> GetDownloadUrlFromArticlePage(string articleUrl);

        Task<IEnumerable<string>> GetAllArticleUrls(string entryUrl);
    }
}
