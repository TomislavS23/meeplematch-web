using meeplematch_web;
using meeplematch_web.Models;
using meeplematch_web_integration_tests.Base;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace meeplematch_web_integration_tests.Controllers
{
    public class AuthTests : BaseTest
    {
        public AuthTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Login_Success_ReturnsFoundAndSetsAuthCookie()
        {
            // Arrange
            //_client.Protected()
            _factory.HttpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("/auth/login?username=test&password=pass")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("mock-jwt-token")
                });
            _factory.HttpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("/user/public/test")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new PublicUserViewModel
                    {
                        IdUser = 1,
                        Username = "test"
                    }), Encoding.UTF8, "application/json")
                });

            _factory.HttpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("/user/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new UserViewModel
                    {
                        IdUser = 1,
                        Username = "test",
                        RoleId = 2
                    }), Encoding.UTF8, "application/json")
                });


        //    var client = _factory.CreateDefaultClient();
        //    var content = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string, string>("username", "test"),
        //        new KeyValuePair<string, string>("password", "pass")
        //    });

        //    // Act
        //    var response = await client.PostAsync("/Auth/Login", content);

        //    // Assert
        //    Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        //    Assert.Contains(".AspNetCore.MyCookieAuth", response.Headers.GetValues("Set-Cookie").FirstOrDefault());
        //}

        [Fact]
        public async Task Login_Fail_ReturnsUnauthorized()
        {
            // Arrange
            //_client.Protected()
            _factory.HttpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            var client = _factory.CreateDefaultClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", "test"),
                new KeyValuePair<string, string>("password", "pass")
            });

            // Act
            var response = await client.PostAsync("/Auth/Login", content);
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains("Invalid credentials", html);
        }
    }
}
