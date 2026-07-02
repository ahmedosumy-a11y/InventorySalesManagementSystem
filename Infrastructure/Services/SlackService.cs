using System.Text;
using System.Text.Json;
using InventorySalesManagementSystem.Application.Interfaces;
using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.Infrastructure.Services;

public class SlackService(
    HttpClient httpClient,
    IConfiguration configuration)
    : ISlackService
{
    public async Task SendAsync(
        SlackChannelType type,
        string title,
        string message)
    {
        var webhookUrl = GetWebhook(type);

        var payload = new
        {
            text =
                $"""
                *{title}*

                {message}

                Time: {DateTime.UtcNow}
                """
        };

        var json = JsonSerializer.Serialize(payload);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        await httpClient.PostAsync(
            webhookUrl,
            content
        );
    }

    private string GetWebhook(
        SlackChannelType type)
    {
        return type switch
        {
            SlackChannelType.Errors =>
                configuration["Slack:ErrorsWebhook"]!,

            SlackChannelType.Sales =>
                configuration["Slack:SalesWebhook"]!,

            SlackChannelType.Inventory =>
                configuration["Slack:InventoryWebhook"]!,

            _ => throw new Exception(
                "Invalid Slack channel")
        };
    }
}