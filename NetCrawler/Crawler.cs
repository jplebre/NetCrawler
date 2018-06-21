using NetCrawler.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCrawler
{
    public class Crawler : ICrawler
    {
        public ConcurrentDictionary<string, WebPage> WebsiteMap { get; set; }
        public Queue<string> LinksToCrawl { get; set; }
        public Uri HostUrl { get; set; }

        private IWebPageParser _parser;
        private int ConcurrencyLimit = 10;

        public Crawler(IWebPageParser parser)
        {
            WebsiteMap = new ConcurrentDictionary<string, WebPage>();
            LinksToCrawl = new Queue<string>();

            _parser = parser;
        }

        public async Task Execute(string hostname)
        {
            await RunCrawl(hostname);
        }

        public ConcurrentDictionary<string, WebPage> GetWebsiteMap() => WebsiteMap;

        private async Task RunCrawl(string hostname)
        {
            HostUrl = VerifyUrlIntegrity(hostname);

            LinksToCrawl.Enqueue(HostUrl.OriginalString);

            while (LinksToCrawl.Any())
            {
                List<Task> tasks = new List<Task>();

                for (var thread = 1; thread <= ConcurrencyLimit && thread <= LinksToCrawl.Count; thread++)
                {
                    var link = new Uri(LinksToCrawl.Dequeue());
                    if (WebsiteMap.ContainsKey(link.Host + link.AbsolutePath)) continue;
                    tasks.Add(CrawlPage(link.OriginalString));
                }

                await Task.WhenAll(tasks);
            }

            Console.WriteLine($"Found {WebsiteMap.Count} links");
        }

        private async Task CrawlPage(string link)
        {
            var pageResults = await _parser.ParsePage(link);

            HandlePageResultLinks(pageResults);

            WebsiteMap.TryAdd(pageResults.PageUrl.Host + pageResults.PageUrl.AbsolutePath, pageResults);

            pageResults.Links.ForEach(x => LinksToCrawl.Enqueue(x));
        }

        public void HandlePageResultLinks(WebPage pageResults)
        {
            List<string> processedLinks = new List<string>();

            foreach (var link in pageResults.Links)
            {
                if (link == null) continue;

                try
                {
                    var uri = new Uri(link, UriKind.RelativeOrAbsolute);

                    if (uri.IsAbsoluteUri)
                    {
                        if (uri.DnsSafeHost == HostUrl.DnsSafeHost)
                        {
                            processedLinks.Add(uri.OriginalString.TrimEnd('/'));
                        }
                    }
                    else
                    {
                        var trimmedLink = uri.OriginalString.TrimEnd('/').TrimStart('/');

                        if (trimmedLink.StartsWith("../"))
                        {
                            processedLinks.Add(new Uri(pageResults.PageUrl, trimmedLink).OriginalString);
                            continue;
                        }
                        else
                        {
                            processedLinks.Add(HostUrl + trimmedLink);
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: the current link: {link} is not a valid Uri");
                }
            }

            pageResults.Links = processedLinks;
        }

        private static Uri VerifyUrlIntegrity(string hostname)
        {
            try
            {
                return new Uri(hostname);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Error: Url ""{hostname}"" is malformed");
                throw;
            }
        }
    }
}
