using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetCrawler
{
    public interface ICrawler
    {
        ConcurrentDictionary<string, WebPage> GetWebsiteMap();
        Task Execute(string hostname);
    }
}