using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.Notifications.Services;
using Notification.Infrastructure.Options;
using Notification.Infrastructure.Services;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Notification.Infrastructure.IntegrationTests;

[TestClass]
public sealed class ResendEmailGatewayTests
{
    [TestMethod]
    public async Task SendAsync_WithSnapshot_PreservesSenderAndIdempotencyKey()
    {
        string? capturedKey = null;
        string? capturedBody = null;
        using var handler = new StubHttpHandler(async request =>
        {
            capturedKey = request.Headers.GetValues("Idempotency-Key").Single();
            capturedBody = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"email-1\"}")
            };
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://resend.test/") };
        var gateway = CreateGateway(client);

        EmailSendResult result = await gateway.SendAsync(new EmailSendRequest(
            Guid.NewGuid(), "recipient@example.com", "Recipient", "Subject", "Text", "<p>Text</p>",
            "notification-dispatch/test", new EmailSender("Frozen <frozen@example.com>", null)));

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("notification-dispatch/test", capturedKey);
        using JsonDocument json = JsonDocument.Parse(capturedBody!);
        Assert.AreEqual("Frozen <frozen@example.com>", json.RootElement.GetProperty("from").GetString());
        Assert.AreEqual(JsonValueKind.Null, json.RootElement.GetProperty("reply_to").ValueKind);
        Assert.AreEqual("email-1", result.ProviderMessageId);
    }

    [TestMethod]
    [DataRow(408, "request_timeout", true)]
    [DataRow(429, "rate_limit_exceeded", true)]
    [DataRow(503, "application_error", true)]
    [DataRow(409, "concurrent_idempotent_requests", true)]
    [DataRow(409, "invalid_idempotent_request", false)]
    [DataRow(400, "validation_error", false)]
    [DataRow(401, "restricted_api_key", false)]
    [DataRow(422, "validation_error", false)]
    public async Task SendAsync_ClassifiesProviderErrors(int status, string errorCode, bool transient)
    {
        using var handler = new StubHttpHandler(_ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { name = errorCode, message = "Failed" })),
            Headers = { RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(60)) }
        }));
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://resend.test/") };

        EmailSendResult result = await CreateGateway(client).SendAsync(
            new EmailSendRequest(Guid.NewGuid(), "to@example.com", null, "Subject", "Body", "HTML"));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(transient, result.IsTransientFailure);
        Assert.AreEqual(errorCode, result.FailureCode);
        Assert.AreEqual(TimeSpan.FromSeconds(60), result.RetryAfter);
    }

    private static ResendEmailGateway CreateGateway(HttpClient client) => new(
        client, OptionsFactory.Create(new NotificationEmailOptions
        {
            ApiKey = "test-key", FromEmail = "configured@example.com", ReplyTo = "changed@example.com"
        }), NullLogger<ResendEmailGateway>.Instance);

    [TestMethod]
    [DataRow("not-json")]
    [DataRow("{}")]
    public async Task SendAsync_WhenSuccessResponseIsAmbiguous_AllowsIdempotentRetry(string body)
    {
        using var handler = new StubHttpHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body)
        }));
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://resend.test/") };
        EmailSendResult result = await CreateGateway(client).SendAsync(
            new EmailSendRequest(Guid.NewGuid(), "to@example.com", null, "Subject", "Body", "HTML"));

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsTransientFailure);
        Assert.AreEqual("invalid_provider_response", result.FailureCode);
    }

    private sealed class StubHttpHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
}
