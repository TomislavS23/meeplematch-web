using meeplematch_web;
using Microsoft.AspNetCore.Mvc.Testing;

namespace meeplematch_web_integration_tests.Base
{
    public partial class BaseTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        protected readonly CustomWebApplicationFactory<Program> _factory;
        protected readonly HttpClient _client;

        public BaseTest(CustomWebApplicationFactory<Program> factory)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            //_client = new Mock<HttpMessageHandler>();
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            // The below code is used as backup and needs to replace everything after the _client = factory
            //.WithWebHostBuilder(builder =>
            //{
            //    builder.ConfigureTestServices(services =>
            //    {
            //        services.AddAuthentication("MyCookieAuth");
            //        services.AddSession(options =>
            //        {
            //            options.IdleTimeout = TimeSpan.FromSeconds(10);
            //            options.Cookie.HttpOnly = true;
            //            options.Cookie.IsEssential = true;
            //        });
            //        services.AddSingleton<IHttpClientFactory>(_ =>
            //        {
            //            var client = new HttpClient(_client.Object)
            //            {
            //                BaseAddress = new Uri("http://localhost:5202/api/meeplematch/")
            //            };
            //            var mockFactory = new Mock<IHttpClientFactory>();
            //            mockFactory.Setup(x => x.CreateClient(Constants.ApiName)).Returns(client);
            //            return mockFactory.Object;
            //        });
            //    });
            //});
        }
    }
}
