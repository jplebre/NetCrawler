using NetCrawler.Boundaries;
using NetCrawler.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCrawler.Tests
{
    [TestFixture]
    public class WhenCreatingRestClient
    {
        [Test]
        public async Task HttpClientIsSetupCorrectly()
        {
            var httpHandler = Substitute.For<MockHttpHandler>();
            var httpClient = new HttpClient(httpHandler);

            var sut = new RestClient(httpClient);

            Assert.That(httpClient.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }
    }
}
