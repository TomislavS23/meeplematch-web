using meeplematch_web;
using meeplematch_web.Models;
using meeplematch_web_integration_tests.Base;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace meeplematch_web_integration_tests.Controllers
{
    public class EventTests : BaseTest
    {
        public EventTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task EventIndex_Success_ReturnsOK()
        {
            var events = new List<EventViewModel>
            {
                new EventViewModel { IdEvent = 1, Name = "Board Game Night", Game = "Catan", CreatedBy = 1, EventDate = DateTime.UtcNow }
            };

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(events), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains("events")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains("user/public")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new PublicUserViewModel { IdUser = 1, Username = "TestUser" }), Encoding.UTF8, "application/json")
                });

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"event-participant/{events.First().IdEvent}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    //Content = new StringContent(JsonSerializer.Serialize(new List<EventParticipantViewModel>().Add(new EventParticipantViewModel { IdEventParticipant = 1, IdEvent = events.First().IdEvent, IdUser = 1})), Encoding.UTF8, MediaTypeNames.Application.Json)
                    Content = new StringContent(JsonSerializer.Serialize(new List<EventParticipantViewModel>()), Encoding.UTF8, MediaTypeNames.Application.Json)
                });

            // Act
            var response = await _client.GetAsync("/Event");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var eventItem = await response.Content.ReadAsStringAsync();
            Assert.Contains(events.First().Name, eventItem);
        }
    }
}
