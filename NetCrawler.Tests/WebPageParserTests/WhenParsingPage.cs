using NetCrawler.Boundaries;
using NetCrawler.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetCrawler.Tests.WebPageParserTests
{
    [TestFixture]
    public class WhenParsingPage
    {
        private const string absoluteUri = "http://testsite.com/home";
        private Uri callUri = new Uri(absoluteUri);

        [Test]
        public async Task CorrectlyParsesAPageWithLinks()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<a href=""testsite.com/path"">stuff</a>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(1));
            Assert.That(result.Links.First(), Is.EqualTo("testsite.com/path"));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithRepeatedLinks()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<html><body><a href=""testsite.com/path"">stuff</a><a href=""testsite.com/path2"">stuff</a><a href=""testsite.com/path2"">stuff</a></html></body>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(2));
            Assert.That(result.Links[0], Is.EqualTo("testsite.com/path"));
            Assert.That(result.Links[1], Is.EqualTo("testsite.com/path2"));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithMailTo()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<html><body><a href=""testsite.com/path"">stuff</a><a href=""mailto:blah"">stuff</a><a href=""testsite.com/path2"">stuff</a></html></body>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(2));
            Assert.That(result.Links[0], Is.EqualTo("testsite.com/path"));
            Assert.That(result.Links[1], Is.EqualTo("testsite.com/path2"));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithNullFields()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<html><body><a>stuff</a></body></html>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(0));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithNoLink()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<html><body><h1>No Links!!</h1></body></html>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(0));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithEmptyHrefFields()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<html><body><a href="">stuff</a></body></html>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(0));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithNoLinks()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<h1>This is not the tag you are looking for</h1>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(0));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithLinksToElements()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<a href=""testsite.com/path"">stuff</a><a href=""testsite.com/path#1"">stuff</a><a href=""#element"">stuff</a>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(2));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task CorrectlyParsesAPageWithLinksToCloudflareFiles()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Returns(@"<a href=""testsite.com/cdn-cgi/path"">stuff</a><a href=""cdn-cgi/path#1"">stuff</a><a href=""testsite.com/path"">stuff</a>");

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(1));
            Assert.That(result.Links.First(), Is.EqualTo("testsite.com/path"));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.False);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task HandlesTimeoutExceptions()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Throws(new RestRequestTimedOutException());

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(0));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.True);

            await restClientMock.Received(2).GetAsync(Arg.Any<Uri>());
        }

        [Test]
        public async Task HandlesBadCalls()
        {
            var restClientMock = Substitute.For<IRestClient>();
            restClientMock.GetAsync(callUri).Throws(new RestRequestFailedException());

            var sut = new WebPageParser(restClientMock);
            var result = await sut.ParsePage(callUri.OriginalString);

            Assert.That(result, Is.TypeOf(typeof(WebPage)));
            Assert.That(result.PageUrl.OriginalString, Is.EqualTo(absoluteUri));
            Assert.That(result.Links.Count, Is.EqualTo(0));
            Assert.That(result.WasCrawled, Is.True);
            Assert.That(result.DeadLink, Is.True);

            await restClientMock.Received(1).GetAsync(Arg.Any<Uri>());
        }
    }
}
