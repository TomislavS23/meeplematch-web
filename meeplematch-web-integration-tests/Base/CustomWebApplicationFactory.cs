using meeplematch_web.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace meeplematch_web_integration_tests.Base
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public Mock<HttpMessageHandler> HttpMessageHandlerMock { get; } = new Mock<HttpMessageHandler>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IHttpClientFactory));

                var client = new HttpClient(HttpMessageHandlerMock.Object)
                {
                    BaseAddress = new Uri("http://localhost:5202/api/meeplematch/")
                };

                var mockFactory = new Mock<IHttpClientFactory>();
                mockFactory.Setup(x => x.CreateClient(Constants.ApiName)).Returns(client);
                services.AddSingleton(mockFactory.Object);
            });
        }
    }
}
