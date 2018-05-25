using NetCrawler.Boundaries;
using NetCrawler.Services;
using NetCrawler.Tests.Examples;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NetCrawler.Tests.EndToEndTests
{
    [TestFixture]
    public class WhenCrawlingHosts
    {
        private IRestClient restClientMock;
        private IWebPageParser webPageParser;
        private ICrawler sut;

        [SetUp]
        public void Setup()
        {
            restClientMock = Substitute.For<IRestClient>();
            webPageParser = new WebPageParser(restClientMock);

            sut = new Crawler(webPageParser);
        }

        [Test]
        public async Task CanCrawlSimpleSite()
        {
            restClientMock.GetAsync(new Uri("http://simplesite.com")).Returns(FileHelper.GetSiteFromFile(@"Examples\SimpleSite\simplesite_home.html"));
            restClientMock.GetAsync(new Uri("http://simplesite.com/about")).Returns(FileHelper.GetSiteFromFile(@"Examples\SimpleSite\simplesite_about.html"));
            restClientMock.GetAsync(new Uri("http://simplesite.com/contact")).Returns(FileHelper.GetSiteFromFile(@"Examples\SimpleSite\simplesite_contact.html"));
            restClientMock.GetAsync(new Uri("http://simplesite.com/hiring")).Returns(FileHelper.GetSiteFromFile(@"Examples\SimpleSite\simplesite_hiring.html"));

            await sut.Execute("http://simplesite.com");

            Assert.That(sut.GetWebsiteMap().Count, Is.EqualTo(4));
        }

        [Test]
        public async Task CanCrawlRecursiveSite()
        {
            restClientMock.GetAsync(new Uri("http://recursivesite.com")).Returns(FileHelper.GetSiteFromFile(@"Examples\RecursiveSite\recursivesite_home.html"));
            restClientMock.GetAsync(new Uri("http://recursivesite.com/about")).Returns(FileHelper.GetSiteFromFile(@"Examples\RecursiveSite\recursivesite_about.html"));
            restClientMock.GetAsync(new Uri("http://recursivesite.com/contact")).Returns(FileHelper.GetSiteFromFile(@"Examples\RecursiveSite\recursivesite_contact.html"));
            restClientMock.GetAsync(new Uri("http://recursivesite.com/hiring")).Returns(FileHelper.GetSiteFromFile(@"Examples\RecursiveSite\recursivesite_hiring.html"));

            await sut.Execute("http://recursivesite.com");

            Assert.That(sut.GetWebsiteMap().Count, Is.EqualTo(4));
        }

        [Test]
        public async Task CanCrawlSite_AndIgnoreExternalLinks()
        {
            restClientMock.GetAsync(new Uri("http://externallinks.com")).Returns(FileHelper.GetSiteFromFile(@"Examples\SiteWithExternalLinks\externallinks_home.html"));
            restClientMock.GetAsync(new Uri("http://externallinks.com/hiring")).Returns(FileHelper.GetSiteFromFile(@"Examples\SiteWithExternalLinks\externallinks_hiring.html"));

            await sut.Execute("http://externallinks.com");

            Assert.That(sut.GetWebsiteMap().Count, Is.EqualTo(2));
        }

        [Test]
        public async Task CanCrawlSite_AndIgnoreDeadLinks()
        {
            restClientMock.GetAsync(new Uri("http://deadlinks.com")).Returns(FileHelper.GetSiteFromFile(@"Examples\SiteWithDeadLinks\deadlinks_home.html"));
            restClientMock.GetAsync(new Uri("http://deadlinks.com/hiring")).Returns(FileHelper.GetSiteFromFile(@"Examples\SiteWithDeadLinks\deadlinks_hiring.html"));
            restClientMock.GetAsync(new Uri("http://deadlinks.com/dead")).Throws(new RestRequestFailedException());

            await sut.Execute("http://deadlinks.com");

            WebPage pageResult;
            if (sut.GetWebsiteMap().TryGetValue("http://deadlinks.com/dead", out pageResult))
            {
                Assert.That(pageResult.DeadLink, Is.True);
            }
            Assert.That(sut.GetWebsiteMap().Count, Is.EqualTo(3));
        }
    }
}
