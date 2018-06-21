using NetCrawler.Boundaries;
using NetCrawler.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCrawler.Tests.RestClientTests
{
    [TestFixture]
    public class WhenCallingGetAsync
    {
        private Uri _testAddress;

        private MockHttpHandler _mockHttpHandler;
        private HttpClient _httpClient;
        private RestClient _sut;
        private string _expectedUrl;

        [SetUp]
        public void Setup()
        {
            _testAddress = new Uri("http://TestHost.com/blah");
            _expectedUrl = _testAddress.OriginalString;

            _mockHttpHandler = Substitute.ForPartsOf<MockHttpHandler>();
            _httpClient = new HttpClient(_mockHttpHandler);

            _sut = new RestClient(_httpClient);
        }

        [Test]
        public async Task NetworkCall_IsDone_WithCorrectParameters()
        {
            _mockHttpHandler.SendAsync(Arg.Any<HttpRequestMessage>())
                .Returns(MockResponseMessage());

            string result = await _sut.GetAsync(_testAddress);

            Assert.That(result, Is.TypeOf(typeof(string)));
            await _mockHttpHandler.Received().SendAsync(
                Arg.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri == new Uri(_expectedUrl)));
        }

        [Test]
        public async Task NetworkCallTimesOut_ExceptionsAreHandledCorrectly()
        {
            _mockHttpHandler.SendAsync(Arg.Any<HttpRequestMessage>()).Throws(new TaskCanceledException());

            Assert.ThrowsAsync<RestRequestTimedOutException>(async () => await _sut.GetAsync(_testAddress));
            await _mockHttpHandler.Received().SendAsync(
                Arg.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri == new Uri(_expectedUrl)));
        }

        [Test]
        public async Task NetworkCallIsCancelled_ExceptionsAreHandledCorrectly()
        {
            _mockHttpHandler.SendAsync(Arg.Any<HttpRequestMessage>()).Throws(new OperationCanceledException());

            Assert.ThrowsAsync<RestRequestTimedOutException>(async () => await _sut.GetAsync(_testAddress));
            await _mockHttpHandler.Received().SendAsync(
                Arg.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri == new Uri(_expectedUrl)));
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.BadRequest)]
        public async Task NonSuccessStatusCode_AreHandledCorrectly(HttpStatusCode statusCode)
        {
            _mockHttpHandler.SendAsync(Arg.Any<HttpRequestMessage>()).Returns(new HttpResponseMessage()
            {
                StatusCode = statusCode
            });

            Assert.ThrowsAsync<RestRequestFailedException>(async () => await _sut.GetAsync(_testAddress));
            await _mockHttpHandler.Received().SendAsync(
                Arg.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Get && x.RequestUri == new Uri(_expectedUrl)));
        }

        private static HttpResponseMessage MockResponseMessage()
        {
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"1")
            };
        }
    }
}
