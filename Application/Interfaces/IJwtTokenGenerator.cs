using InventorySalesManagementSystem.Domain.Entities;

namespace InventorySalesManagementSystem.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
