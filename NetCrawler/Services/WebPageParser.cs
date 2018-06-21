using HtmlAgilityPack;
using NetCrawler.Boundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NetCrawler.Services
{
    public class WebPageParser : IWebPageParser
    {
        private const int Retries = 2;
        private readonly IRestClient _restClient;

        public WebPageParser(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<WebPage> ParsePage(string url)
        {
            var webPage = new WebPage();
            webPage.PageUrl = new Uri(url);

            try
            {
                return await GetPageWithRetries(webPage);
            }
            catch (RestRequestFailedException ex)
            {
                webPage.WasCrawled = true;
                webPage.DeadLink = true;

                return webPage;
            }
        }

        private async Task<WebPage> GetPageWithRetries(WebPage webPage)
        {
            for (int i = 1; i <= Retries; i++)
            {
                try
                {
                    var pageContent = await _restClient.GetAsync(webPage.PageUrl);

                    webPage.Links = ExtractLinksFromPage(pageContent) ?? new List<string>();
                    webPage.WasCrawled = true;
                    webPage.DeadLink = false;

                    break;
                }
                catch (RestRequestTimedOutException ex)
                {
                    if (i >= Retries) throw new RestRequestFailedException();
                }
            }

            return webPage;
        }

        private List<string> ExtractLinksFromPage(string pageContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);

            List<string> links = htmlDoc.DocumentNode
                .SelectNodes("//a")?
                .Select(x => HttpUtility.HtmlDecode(x.Attributes["href"]?.Value))?
                .Where(href => href != null)
                .Where(href => !href.StartsWith("mailto:"))         //Ignore mailto: links
                .Where(href => !href.StartsWith("#"))               //Ignore links to the same page
                .Where(href => !href.Contains("cdn-cgi"))           //Ignore cloudflare links
                .Distinct()
                .ToList();

            return links;
        }
    }
}
