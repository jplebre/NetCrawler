using System;
using System.Threading.Tasks;

namespace NetCrawler.Boundaries
{
    public interface IRestClient
    {
        Task<string> GetAsync(Uri uri);
    }
}