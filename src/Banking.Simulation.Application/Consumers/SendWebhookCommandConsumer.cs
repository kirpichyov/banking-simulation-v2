using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Banking.Simulation.Core.Models.Enums;
using Banking.Simulation.Core.Models.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Banking.Simulation.Application.Consumers;

public sealed class SendWebhookCommandConsumer : IConsumer<SendWebhookCommand>
{
    private const string JsonContentType = "application/json";
    private const string SecretHeaderName = "Webhook-Secret";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SendWebhookCommandConsumer> _logger;

    public SendWebhookCommandConsumer(
        IHttpClientFactory httpClientFactory,
        ILogger<SendWebhookCommandConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendWebhookCommand> context)
    {
        ArgumentNullException.ThrowIfNull(context.Message, nameof(context.Message));

        var data = new WebhookData()
        {
            PaymentId = context.Message.PaymentId,
            PreviousStatus = context.Message.PreviousStatus,
            CurrentStatus = context.Message.CurrentStatus,
        };
  
        await Send(
            context.Message.WebhookUrl,
            context.Message.WebhookSecret,
            data);
    }

    private async Task Send<TData>(string url, string secret, TData data)
    {
        var httpClient = _httpClientFactory.CreateClient();
        HttpRequestMessage request;

        var requestJson = JsonSerializer.Serialize(data, new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() }
        });

        try
        {
            request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonContentType));
            request.Content = new StringContent(requestJson);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(JsonContentType);
            request.Headers.Add(SecretHeaderName, secret);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occured while creating HttpRequestMessage for URL '{Uri}'", url);
            return;
        }

        try
        {
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Webhook successfully sent to '{Url}'", url);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Response code indicates failure for url '{Uri}'", url);
        }
    }

    private sealed record WebhookData()
    {
        public Guid PaymentId { get; init; }
        public PaymentStatus PreviousStatus { get; init; }
        public PaymentStatus CurrentStatus { get; init; }
    }
}