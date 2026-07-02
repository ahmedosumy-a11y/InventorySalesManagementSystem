using InventorySalesManagementSystem.Domain.Enums;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface ISlackService
{
    Task SendAsync(
        SlackChannelType type,
        string title,
        string message
    );
}