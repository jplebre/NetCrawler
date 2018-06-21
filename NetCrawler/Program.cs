using NetCrawler.Boundaries;
using NetCrawler.Services;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCrawler
{
    public class Program
    {
        private static string HostName = "https://www.elysia.com/";

        public static async Task Main(string[] args)
        {
            var crawler = new Crawler(new WebPageParser(new RestClient(new HttpClient())));

            Console.WriteLine("INFO: initiating crawl");

            await TimeAndExecuteOperation(crawler);

            Console.WriteLine("INFO: Outputting Site Map");
            await OutputSiteMap(crawler.GetWebsiteMap());

            Console.WriteLine("INFO: Press Enter To Exit");
            Console.ReadLine();
        }

        private static async Task TimeAndExecuteOperation(Crawler crawler)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            await crawler.Execute(HostName);

            sw.Stop();
            Console.WriteLine($"INFO: Crawled {HostName} in {sw.Elapsed}");
        }

        private static async Task OutputSiteMap(ConcurrentDictionary<string, WebPage> siteMap)
        {
            using (StreamWriter sw = File.AppendText($"websitemap_{DateTime.UtcNow.ToFileTimeUtc()}.txt"))
            { 
                foreach (var page in siteMap)
                {
                    if (page.Value.DeadLink)
                    {
                        string message = $"[BASE ADDRESS] {page.Key} (dead page)";

                        Console.WriteLine(message);
                        sw.WriteLine(message);

                        continue;
                    }

                    Console.WriteLine($"[BASE ADDRESS] {page.Key} =>");
                    sw.WriteLine($"[BASE ADDRESS] {page.Key} =>");

                    foreach (var link in page.Value.Links)
                    {
                        string message = $"---- {link}";

                        Console.WriteLine(message);
                        sw.WriteLine(message);
                    }
                }
            }
        }
    }
}
