using Bogus;
using InventorySalesManagementSystem.Domain.Entities;
using InventorySalesManagementSystem.Domain.Enums;
using InventorySalesManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Warehouses.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Check if database is already seeded
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        // Configure Bogus to be deterministic
        Randomizer.Seed = new Random(8675309);

        // 2. Roles
        var roles = new List<Role>
        {
            new Role { Name = "Admin" },
            new Role { Name = "Manager" },
            new Role { Name = "Staff" },
            new Role { Name = "Customer" }
        };
        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // 3. Users (Admin + Bogus Customers)
        var users = new List<User>
        {
            new User
            {
                FullName = "Admin User",
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = roles[0].Id
            }
        };

        var userFaker = new Faker<User>()
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Password, f => BCrypt.Net.BCrypt.HashPassword("User@123"))
            .RuleFor(u => u.RoleId, f => f.PickRandom(roles.Skip(1).Select(r => r.Id)))
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1));

        users.AddRange(userFaker.Generate(20)); // 20 mock users
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // 4. Warehouses
        var warehouseFaker = new Faker<Warehouse>()
            .RuleFor(w => w.Name, f => f.Company.CompanyName() + " Warehouse")
            .RuleFor(w => w.Address, f => f.Address.FullAddress())
            .RuleFor(w => w.IsActive, true);
        
        var warehouses = warehouseFaker.Generate(3);
        await context.Warehouses.AddRangeAsync(warehouses);
        await context.SaveChangesAsync();

        // 5. Categories
        var categories = new List<Category>
        {
            new Category { Name = "Electronics", Description = "Electronic devices and accessories" },
            new Category { Name = "Clothing", Description = "Apparel and garments" },
            new Category { Name = "Home & Kitchen", Description = "Furniture and kitchenware" },
            new Category { Name = "Books", Description = "Printed and electronic books" }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // 6. Products
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.SKU, f => f.Commerce.Ean8())
            .RuleFor(p => p.Barcode, f => f.Commerce.Ean13())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(20, 1000)))
            .RuleFor(p => p.CostPrice, (f, p) => p.Price * 0.7m) // 30% margin
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(p => p.IsActive, true);

        var products = productFaker.Generate(50);
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // 7. Inventory
        var inventories = new List<Inventory>();
        var inventoryFaker = new Faker();
        foreach (var warehouse in warehouses)
        {
            foreach (var product in products)
            {
                // Only stock some products in some warehouses randomly to make it realistic
                if (inventoryFaker.Random.Bool(0.8f))
                {
                    inventories.Add(new Inventory
                    {
                        ProductId = product.Id,
                        WarehouseId = warehouse.Id,
                        Quantity = inventoryFaker.Random.Number(10, 500),
                        ReservedQuantity = inventoryFaker.Random.Number(0, 5)
                    });
                }
            }
        }
        await context.Inventories.AddRangeAsync(inventories);
        await context.SaveChangesAsync();

        // 8. Orders & OrderItems & Sales
        var orders = new List<Order>();
        var sales = new List<Sale>();
        
        var rand = new Faker();
        for (int i = 0; i < 30; i++)
        {
            var user = rand.PickRandom(users);
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = rand.Date.Past(1),
                Status = rand.PickRandom<OrderStatus>(),
                OrderItems = new List<OrderItem>()
            };

            var selectedProducts = rand.PickRandom(products, rand.Random.Number(1, 5)).ToList();
            decimal totalAmount = 0;
            
            foreach (var sp in selectedProducts)
            {
                var quantity = rand.Random.Number(1, 4);
                var unitPrice = sp.Price;
                var itemTotal = quantity * unitPrice;
                totalAmount += itemTotal;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = sp.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal
                });
            }

            order.TotalAmount = totalAmount;

            // Generate Sale for Delivered/Shipped orders
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
            {
                var sale = new Sale
                {
                    Order = order, // EF Core handles navigation properties
                    InvoiceNumber = "INV-" + rand.Random.AlphaNumeric(8).ToUpper(),
                    SaleDate = order.OrderDate.AddDays(rand.Random.Number(1, 3)),
                    GrandTotal = totalAmount
                };
                sales.Add(sale);
                order.Sale = sale;
            }

            orders.Add(order);
        }

        await context.Orders.AddRangeAsync(orders);
        await context.Sales.AddRangeAsync(sales);
        await context.SaveChangesAsync();
    }
}
