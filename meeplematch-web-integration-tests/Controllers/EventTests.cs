using meeplematch_web;
using meeplematch_web.Controllers;
using meeplematch_web.Models;
using meeplematch_web_integration_tests.Base;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        [Fact]
        public async Task Index_WithSearchAndPaging_ReturnsFilteredResults()
        {
            // Arrange
            var allEvents = new List<EventViewModel>
        {
            new() { IdEvent = 1, Name = "Board Game Night", Game = "Catan", CreatedBy = 1, EventDate = DateTime.UtcNow },
            new() { IdEvent = 2, Name = "Card Game Night", Game = "Poker", CreatedBy = 2, EventDate = DateTime.UtcNow.AddDays(1) }
        };

            var expectedEvent = allEvents[0];

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(allEvents), Encoding.UTF8, MediaTypeNames.Application.Json)
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
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"event-participant/{expectedEvent.IdEvent}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new List<EventParticipantViewModel>()), Encoding.UTF8, MediaTypeNames.Application.Json)
                });

            // Act
            var response = await _client.GetAsync("/Event?search=board&page=1&pageSize=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedEvent.Name, content);
            Assert.DoesNotContain("Card Game", content);
        }

        [Fact]
        public async Task Details_WithComments_ReturnsPagedComments()
        {
            // Arrange
            var eventId = 1;
            var comments = new List<EventCommentViewModel>
            {
                new EventCommentViewModel { IdEventComment = 1, EventId = eventId, Comment = "Great event!", UserId = 1, CreatedAt = DateTime.UtcNow },
                new EventCommentViewModel { IdEventComment = 2, EventId = eventId, Comment = "Had a lot of fun!", UserId = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-10) }
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(comments), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"events/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new EventViewModel { IdEvent = eventId, Name = "Board Game Night", Game = "Catan", CreatedBy = 1, EventDate = DateTime.UtcNow }), Encoding.UTF8, MediaTypeNames.Application.Json)
                });

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"event-comment/{eventId}")),
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
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"event-participant/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new List<EventParticipantViewModel>()), Encoding.UTF8, MediaTypeNames.Application.Json)
                });


            // Act
            var response = await _client.GetAsync($"/Event/Details/{eventId}");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Great event!", content);
        }

        [Fact]
        public async Task Details_WithAuthenticatedUser_SetsParticipantStatus()
        {
            var eventId = 1;
            var userId = 1;

            var eventViewModel = new EventViewModel
            {
                IdEvent = eventId,
                Name = "Board Game Night",
                Game = "Catan",
                CreatedBy = userId,
                EventDate = DateTime.UtcNow
            };

            var eventParticipant = new List<EventParticipantViewModel>
            {
                new EventParticipantViewModel { IdEvent = eventId, IdUser = userId, IsJoined = true }
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(eventParticipant), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"event-participant/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"events/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(eventViewModel), Encoding.UTF8, MediaTypeNames.Application.Json)
                });

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains("user/public")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new PublicUserViewModel { IdUser = userId, Username = "TestUser" }), Encoding.UTF8, "application/json")
                });

            _factory.HttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"event-comment/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                    )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new List<EventCommentViewModel>()), Encoding.UTF8, MediaTypeNames.Application.Json)
                });

            // Act
            var response = await _client.GetAsync($"/Event/Details/{eventId}");
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Join", content);
        }
    }
}