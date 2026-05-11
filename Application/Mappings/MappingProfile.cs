using AutoMapper;
using InventorySalesManagementSystem.Application.DTOs.Categories;
using InventorySalesManagementSystem.Application.DTOs.Orders;
using InventorySalesManagementSystem.Application.DTOs.Products;
using InventorySalesManagementSystem.Application.DTOs.Roles;
using InventorySalesManagementSystem.Application.DTOs.Sales;
using InventorySalesManagementSystem.Application.DTOs.Users;
using InventorySalesManagementSystem.Application.DTOs.Warehouses;
using InventorySalesManagementSystem.Domain.Entities;

namespace InventorySalesManagementSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Role, RoleDto>();

        CreateMap<User, UserDto>()
            .ForMember(destination => destination.Role, options => options.MapFrom(source => source.Role != null ? source.Role.Name : string.Empty));

        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<Category, CategoryDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(destination => destination.CategoryName, options => options.MapFrom(source => source.Category != null ? source.Category.Name : string.Empty));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(destination => destination.ProductName, options => options.MapFrom(source => source.Product != null ? source.Product.Name : string.Empty));

        CreateMap<Order, OrderDto>()
            .ForMember(destination => destination.UserName, options => options.MapFrom(source => source.User != null ? source.User.FullName : string.Empty))
            .ForMember(destination => destination.WarehouseName, options => options.MapFrom(source => source.Warehouse != null ? source.Warehouse.Name : string.Empty))
            .ForMember(destination => destination.Status, options => options.MapFrom(source => source.Status.ToString()))
            .ForMember(destination => destination.InvoiceNumber, options => options.MapFrom(source => source.Sale != null ? source.Sale.InvoiceNumber : null))
            .ForMember(destination => destination.Items, options => options.MapFrom(source => source.OrderItems));

        CreateMap<Sale, SaleDto>()
            .ForMember(destination => destination.CustomerName, options => options.MapFrom(source => source.Order != null && source.Order.User != null ? source.Order.User.FullName : string.Empty))
            .ForMember(destination => destination.WarehouseName, options => options.MapFrom(source => source.Order != null && source.Order.Warehouse != null ? source.Order.Warehouse.Name : string.Empty));
    }
}
