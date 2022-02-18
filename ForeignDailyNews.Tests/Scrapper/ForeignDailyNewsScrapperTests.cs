using System.Threading.Tasks;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using Xunit;

namespace ForeignDailyNews.Tests.Scrapper
{
    public class ForeignDailyNewsScrapperTests
    {
        private ForeignDailyNews.Scrapper.Scrapper _sut;

        public ForeignDailyNewsScrapperTests()
        {
            _sut = new ForeignDailyNews.Scrapper.Scrapper();
            new DriverManager().SetUpDriver(new ChromeConfig());
        }


        public class WhenGetDownloadUrlFromArticlePageIsCalled : ForeignDailyNewsScrapperTests
        {
            [Theory]
            [InlineData("http://www.dzialzagraniczny.pl/2022/02/czemu-indie-zakazaly-surogacji-dzial-zagraniczny-podcast115/", "https://traffic.libsyn.com/secure/forcedn/dzialzagraniczny/137_-_INDIE_-_WERONIKA_ROKICKA.mp3")]
            [InlineData("http://www.dzialzagraniczny.pl/2022/02/czego-nie-pokazuja-serbskie-media-dzial-zagraniczny-podcast116/", "https://traffic.libsyn.com/secure/forcedn/dzialzagraniczny/138_-_SERBIA_-_MARTA_SZPALA.mp3")]
            public async Task ItShouldFindDownloadUrlFromEmbedPlayerPage(string articleUrl, string expectedDownloadUrl)
            {
                var actualDownloadUrl = await _sut.GetDownloadUrlFromArticlePage(articleUrl);
                Assert.Equal(expectedDownloadUrl, actualDownloadUrl);
            }

            [Theory]
            [InlineData("http://www.dzialzagraniczny.pl/2019/07/jak-kanada-przetracila-zycie-setkom-tysiecy-dzieci-dzial-zagraniczny-podcast001/", "https://traffic.libsyn.com/secure/forcedn/dzialzagraniczny/001_-_KANADA_-_JOANNA_GIERAK_ONOSZKO.mp3")]
            [InlineData("http://www.dzialzagraniczny.pl/2022/01/czy-w-turcji-czerkieska-tozsamosc-ulega-rozmyciu-dzial-zagraniczny-podcast113/", "https://traffic.libsyn.com/secure/forcedn/dzialzagraniczny/135_-_TURCJA_-_ADAM_BALCER.mp3")]
            public async Task ItShouldFindDownloadUrlFromLibsynPlayerPage(string articleUrl, string expectedDownloadUrl)
            {
                var actualDownloadUrl = await _sut.GetDownloadUrlFromArticlePage(articleUrl);
                Assert.Equal(expectedDownloadUrl, actualDownloadUrl);
            }

            [Theory]
            [InlineData("http://www.dzialzagraniczny.pl/2022/01/jak-malediwy-nie-radza-sobie-ze-smieciami-dzial-zagraniczny-podcast114/")]
            public async Task ItShouldReturnNullForNotHandledPlayers(string articleUrl)
            {
                var actualDownloadUrl = await _sut.GetDownloadUrlFromArticlePage(articleUrl);
                Assert.Null(actualDownloadUrl);
            }

        }
    }
}
