using System.Threading.Tasks;

namespace NetCrawler.Services
{
    public interface IWebPageParser
    {
        Task<WebPage> ParsePage(string url);
    }
}