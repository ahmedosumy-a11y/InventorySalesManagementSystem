# CQRS Migration Plan

## 1. Current Architecture Analysis

The project currently follows Clean Architecture with a clear high-level separation:

- `API` owns controllers and exception middleware.
- `Application` owns DTOs, service interfaces, service implementations, validators, and AutoMapper configuration.
- `Domain` owns entities and enums.
- `Infrastructure` owns EF Core, repositories, unit of work, JWT, security, and external service implementations.

This is a good base for CQRS because the Application layer already coordinates use cases through interfaces and DTOs. The main architectural issue is that most Application services are vertical "resource services" that mix read use cases and write use cases in the same class and interface.

The repository and UnitOfWork abstractions are currently used by both reads and writes. For a gradual CQRS migration, that is acceptable. The first migration should separate Application-level responsibilities without changing the Domain layer or replacing Infrastructure.

## 2. CQRS Violations Found

In CQRS terms, a query returns data and should not mutate state. A command changes state and may return a result DTO when needed by the API, but it should be treated as a write use case.

Services currently mixing queries and commands:

- `ProductService`
  - Queries: `GetAllAsync`, `GetByIdAsync`, `GetInventoryAsync`
  - Commands: `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- `CategoryService`
  - Queries: `GetAllAsync`, `GetByIdAsync`
  - Commands: `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- `WarehouseService`
  - Queries: `GetAllAsync`, `GetByIdAsync`
  - Commands: `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- `InventoryService`
  - Queries: `GetByWarehouseAsync`
  - Commands: `AdjustAsync`, `TransferAsync`
- `OrderService`
  - Queries: `GetAllAsync`, `GetByIdAsync`, `GetByIdForUserAsync`, `GetMyOrdersAsync`
  - Commands: `CreateAsync`, `UpdateStatusAsync`
- `SaleService`
  - Queries: `GetByIdAsync`
  - Commands: `GenerateInvoiceAsync`
- `UserService`
  - Queries: `GetAllAsync`, `GetByIdAsync`
  - Commands: `UpdateStatusAsync`, `UpdateRoleAsync`

Services already close to CQRS:

- `ReportService` is query-only and should remain query-oriented.
- `RoleService` is query-only and should remain query-oriented.
- `AuthService` is command/auth workflow oriented. It can remain as-is during early phases because login/register are application workflows, not simple entity reads.

## 3. Recommended CQRS Structure

Use a feature-based structure inside `Application`:

```text
Application/
  Features/
    Products/
      Commands/
        IProductCommandService.cs
        ProductCommandService.cs
      Queries/
        IProductQueryService.cs
        ProductQueryService.cs
    Categories/
      Commands/
      Queries/
    Warehouses/
      Commands/
      Queries/
    Inventory/
      Commands/
      Queries/
    Orders/
      Commands/
      Queries/
    Sales/
      Commands/
      Queries/
    Users/
      Commands/
      Queries/
    Reports/
      Queries/
    Roles/
      Queries/
  Services/
    BusinessRules/
    Shared/
```

Recommended over global `Application/Commands` and `Application/Queries` because this project is resource/use-case oriented. Feature folders keep each module small, make gradual migration easy, and avoid a large flat folder as commands and queries grow.

Do not introduce MediatR in phase 1. Manual CQRS services are enough:

- `IProductQueryService`
- `IProductCommandService`
- `ProductQueryService`
- `ProductCommandService`

MediatR can be considered later if cross-cutting behaviors become valuable, such as validation pipelines, logging pipelines, transactions, audit behaviors, or notification dispatching.

## 4. Service-by-Service Migration Recommendations

### ProductService

Priority: highest. It has clear read/write separation and low migration risk.

Queries:

- `GetAllAsync`
- `GetByIdAsync`
- `GetInventoryAsync`

Commands:

- `CreateAsync`
- `UpdateAsync`
- `DeleteAsync` currently performs soft delete by setting `IsActive = false`.

Keep in FluentValidation:

- `Name` required and max length.
- `SKU` required and max length.
- `Barcode` required and max length.
- `Price` and `CostPrice` non-negative.
- `CategoryId > 0`.

Extract business rules:

- Category existence check.
- Unique SKU check.
- Unique barcode check.
- Product active/existence checks if reused by Inventory and Orders.

Suggested shared service:

- `IProductRules` or `IProductValidationService`
  - `EnsureCategoryExistsAsync`
  - `EnsureUniqueSkuAsync`
  - `EnsureUniqueBarcodeAsync`
  - `EnsureActiveProductExistsAsync`

### CategoryService

Priority: high. Simple CRUD and easy to split after Product.

Queries:

- `GetAllAsync`
- `GetByIdAsync`

Commands:

- `CreateAsync`
- `UpdateAsync`
- `DeleteAsync`

Keep in FluentValidation:

- `Name` required and max length.
- `Description` max length.

Extract business rules:

- Unique category name.
- Cannot delete a category linked to products.

Suggested shared service:

- `ICategoryRules`
  - `EnsureUniqueNameAsync`
  - `EnsureCanDeleteAsync`

### WarehouseService

Priority: high. Similar shape to Category and Product.

Queries:

- `GetAllAsync`
- `GetByIdAsync`

Commands:

- `CreateAsync`
- `UpdateAsync`
- `DeleteAsync` currently soft-deactivates the warehouse.

Keep in FluentValidation:

- `Name` required and max length.
- `Address` required and max length.

Extract business rules:

- Unique warehouse name.
- Active warehouse existence checks reused by Inventory and Orders.
- Future rule: prevent deactivation if warehouse has active inventory or open orders, if business requires it.

Suggested shared service:

- `IWarehouseRules`
  - `EnsureUniqueNameAsync`
  - `EnsureActiveWarehouseExistsAsync`

### InventoryService

Priority: medium-high. Inventory rules are reused by order creation, order shipping, transfer, and adjustment.

Queries:

- `GetByWarehouseAsync`

Commands:

- `AdjustAsync`
- `TransferAsync`

Keep in FluentValidation:

- Adjustment product and warehouse ids > 0.
- Adjustment quantity change not zero.
- Transfer product/from/to warehouse ids > 0.
- Transfer quantity > 0.
- Source and destination warehouses must be different.

Extract business rules:

- Product must be active.
- Warehouse must be active.
- Inventory quantity cannot become negative.
- Quantity cannot fall below reserved quantity.
- Available quantity must be enough for transfer.
- Creating missing inventory records should be centralized.

Suggested shared service:

- `IInventoryRules`
  - `EnsureActiveProductAndWarehouseAsync`
  - `EnsureAvailableQuantityAsync`
  - `EnsureQuantityCanDecreaseAsync`
  - `GetOrCreateInventoryAsync`

This extraction matters because `OrderService` currently repeats inventory availability, reservation, shipment, and release behavior.

### OrderService

Priority: high impact but should happen after Product, Category, Warehouse, and Inventory rules exist. It contains the densest business workflow.

Queries:

- `GetAllAsync`
- `GetByIdAsync`
- `GetByIdForUserAsync`
- `GetMyOrdersAsync`
- `BuildOrderQuery` should move into the query service or a private query helper.

Commands:

- `CreateAsync`
- `UpdateStatusAsync`

Keep in FluentValidation:

- `WarehouseId > 0`.
- `Items` not empty.
- Item product id and quantity rules.
- Status enum validation.
- Add `OrderSource` enum validation if not already covered by DTO defaults.

Extract business rules:

- Active user exists.
- Active warehouse exists.
- All products exist and are active.
- Duplicate order items should be grouped once.
- Available inventory must satisfy requested quantities.
- POS orders immediately reduce inventory and become delivered.
- Online orders reserve inventory and start pending.
- Valid order status transitions.
- Shipped orders consume reserved inventory.
- Cancelled orders release reserved inventory.
- Delivered orders create sales/invoices.
- Slack notification should stay outside core order rules and be invoked by command workflow after transaction success.

Suggested shared services:

- `IOrderStatusRules`
  - `ValidateTransition`
- `IOrderInventoryService`
  - `ReserveForOrderAsync`
  - `ShipOrderAsync`
  - `ReleaseReservedInventoryAsync`
  - `ConsumeForPosOrderAsync`
- `IOrderPricingService`
  - calculate item totals and order total.
- `ISaleCreationService`
  - create sale for delivered/POS orders and assign invoice numbers.
- `IOrderSourceStrategy`
  - optional later if POS/Online behavior grows. For now, a small private method or switch is enough.

### SaleService

Priority: medium. It is small but overlaps with order delivery logic.

Queries:

- `GetByIdAsync`

Commands:

- `GenerateInvoiceAsync`

Keep in FluentValidation:

- If a request DTO is introduced later, validate `OrderId > 0`.

Extract business rules:

- Invoice can only be generated for delivered orders.
- Existing sale should be returned instead of duplicating sale.
- Invoice number generation format should be centralized.

Suggested shared service:

- `ISaleCreationService`
  - `GetOrCreateSaleForDeliveredOrderAsync`
  - `EnsureInvoiceNumberAsync`

This avoids duplicating sale creation between `OrderService` and `SaleService`.

### UserService

Priority: medium-low. Useful split but less domain-heavy.

Queries:

- `GetAllAsync`
- `GetByIdAsync`

Commands:

- `UpdateStatusAsync`
- `UpdateRoleAsync`

Keep in FluentValidation:

- `UpdateUserStatusDto` validation.
- `UpdateUserRoleDto` validation.

Extract business rules:

- User must exist.
- Role must exist.
- Future rule: prevent disabling the last admin, if needed.

Suggested shared service:

- `IUserRules`
  - `EnsureUserExistsAsync`
  - `EnsureRoleExistsAsync`

### ReportService

Priority: keep as query-only.

Queries:

- `GetDailySalesAsync`
- `GetMonthlySalesAsync`
- `GetTopProductsAsync`

Commands:

- None.

Keep in FluentValidation:

- Optional future query validators for report parameters:
  - `count` should have a maximum.
  - `month` should be 1-12.
  - `year` should be in an acceptable range.

Recommendation:

- Move under `Features/Reports/Queries` when ready.
- Do not create a command service for reports.

### RoleService

Role service is query-only and can move later to `Features/Roles/Queries`.

### AuthService

Keep outside early CQRS migration. `RegisterAsync` and `LoginAsync` are application workflows. If migrated later:

- `RegisterAsync` is a command.
- `LoginAsync` is an auth command/query hybrid because it reads a user and issues a token. Treat it as an Auth command workflow rather than forcing it into product-style CQRS.

## 5. Reusable Business Logic Extraction Opportunities

Extract these before or during command splits:

- Product validation
  - Unique SKU.
  - Unique barcode.
  - Category existence.
  - Active product existence.
- Category rules
  - Unique category name.
  - Prevent deletion when products exist.
- Warehouse rules
  - Unique warehouse name.
  - Active warehouse existence.
- Inventory checks
  - Available quantity calculation.
  - Quantity cannot be negative.
  - Quantity cannot be below reserved quantity.
  - Product/warehouse active checks.
  - Get or create destination inventory record.
- Order status transition rules
  - Delivered and cancelled orders are terminal.
  - Pending can move to approved, shipped, or cancelled.
  - Approved can move to shipped or cancelled.
  - Shipped can move to delivered.
- Order source behavior
  - POS: delivered immediately, consume inventory, create sale.
  - Online: pending, reserve inventory.
  - Use a simple switch first. Introduce strategy classes only when more order sources or source-specific rules appear.
- Sale creation logic
  - Create sale for delivered orders.
  - Avoid duplicate sale creation.
  - Generate invoice numbers consistently.
  - Reuse from order status update, POS order creation, and sales invoice generation.

## 6. Interface Migration Recommendations

Do not remove current interfaces immediately. Keep them temporarily to avoid breaking controllers and dependency injection.

Recommended transition:

1. Introduce split interfaces beside existing interfaces:
   - `IProductQueryService`
   - `IProductCommandService`
   - `ICategoryQueryService`
   - `ICategoryCommandService`
   - and so on.
2. Keep existing `IProductService`, `IOrderService`, etc. as compatibility facades during migration.
3. Make old service implementations delegate to the new command/query services, or update controllers directly one feature at a time.
4. Once all controllers use split services, remove the old resource service interfaces and implementations.

Suggested final state:

- Remove mixed interfaces:
  - `IProductService`
  - `ICategoryService`
  - `IWarehouseService`
  - `IInventoryService`
  - `IOrderService`
  - `ISaleService`
  - `IUserService`
- Keep query-only interfaces:
  - `IReportQueryService`
  - `IRoleQueryService`
- Keep infrastructure/application support interfaces:
  - `IUnitOfWork`
  - `IGenericRepository`
  - `IJwtTokenGenerator`
  - `IPasswordHasher`
  - `ISlackService`

## 7. Validation Strategy

Keep FluentValidation for input shape and simple DTO rules:

- Required fields.
- Length limits.
- Numeric ranges.
- Enum validity.
- Collection not empty.
- Cross-property DTO checks, such as transfer source and destination being different.

Move database-dependent and state-dependent checks to Application business rule services:

- Entity existence.
- Active/inactive checks.
- Uniqueness checks.
- Inventory availability.
- Order status transitions.
- Sale/invoice eligibility.

Do not put EF Core queries inside FluentValidation during phase 1. Keeping those checks inside command services or rule services makes transaction behavior clearer and avoids hidden database work during model binding.

## 8. Controller Migration Strategy

Preserve routes, authorization, request DTOs, response DTOs, and status codes.

Current:

```text
Controller -> Mixed Service
```

Target:

```text
Controller -> QueryService for GET endpoints
Controller -> CommandService for POST/PUT/DELETE endpoints
```

Example for Products:

- `GET /api/products` uses `IProductQueryService.GetAllAsync`.
- `GET /api/products/{id}` uses `IProductQueryService.GetByIdAsync`.
- `GET /api/products/{id}/inventory` uses `IProductQueryService.GetInventoryAsync`.
- `POST /api/products` uses `IProductCommandService.CreateAsync`.
- `PUT /api/products/{id}` uses `IProductCommandService.UpdateAsync`.
- `DELETE /api/products/{id}` uses `IProductCommandService.DeleteAsync`.

Safe migration options:

- Option A: update one controller at a time to inject both query and command services.
- Option B: keep old service as a facade, then switch controllers later.

Option A is cleaner and still safe if done feature-by-feature. Option B is safer if there are many consumers outside controllers.

## 9. Step-by-Step Migration Roadmap

### Phase 1: Product CQRS split

- Create `Features/Products/Queries` and `Features/Products/Commands`.
- Add `IProductQueryService` and `IProductCommandService`.
- Move read methods to `ProductQueryService`.
- Move write methods to `ProductCommandService`.
- Extract product rules for category existence, SKU uniqueness, and barcode uniqueness.
- Update `ProductsController` to inject query and command services.
- Keep `IProductService` temporarily only if needed by other code.

### Phase 2: Category and Warehouse split

- Split `CategoryService` into query and command services.
- Split `WarehouseService` into query and command services.
- Extract unique-name and delete/deactivate rules.
- Update `CategoriesController` and `WarehousesController`.

### Phase 3: Inventory split and shared inventory rules

- Split `InventoryService`.
- Extract active product/warehouse checks.
- Extract available quantity and quantity mutation rules.
- Update `InventoryController`.
- Keep transaction ownership in command services.

### Phase 4: Order command/query split

- Move order read methods into `OrderQueryService`.
- Move `CreateAsync` and `UpdateStatusAsync` into `OrderCommandService`.
- Extract order status transition rules.
- Reuse inventory rule services from phase 3.
- Extract sale creation/invoice generation to avoid duplication.
- Keep Slack notification in command workflow after transaction commit.
- Update `OrdersController`.

### Phase 5: Sale split

- Move `GetByIdAsync` to `SaleQueryService`.
- Move `GenerateInvoiceAsync` to `SaleCommandService`.
- Reuse `ISaleCreationService`.
- Update `SalesController`.

### Phase 6: User split

- Move user read methods to `UserQueryService`.
- Move user write methods to `UserCommandService`.
- Extract user/role existence rules if useful.
- Update `UsersController`.

### Phase 7: Query-only cleanup

- Move `ReportService` to `Features/Reports/Queries`.
- Move `RoleService` to `Features/Roles/Queries`.
- Rename interfaces to `IReportQueryService` and `IRoleQueryService`.

### Phase 8: Remove compatibility services

- Remove old mixed interfaces and service registrations after all controllers use split services.
- Clean up `Program.cs` registrations.
- Keep DTOs and AutoMapper unless direct projection becomes clearly simpler.

### Phase 9: Optional MediatR adoption

Consider MediatR only after manual CQRS is stable and if the project needs:

- Centralized validation pipeline.
- Transaction pipeline behavior.
- Logging/audit pipeline behavior.
- Domain/application event notifications.
- Cleaner orchestration for many commands and queries.

Do not adopt MediatR just to rename services into handlers.

## 10. Risks and Things to Avoid

- Do not modify `Domain/Entities` or `Domain/Enums` for this migration.
- Do not move EF Core implementation details into Domain.
- Do not replace repositories or UnitOfWork during the first CQRS phases.
- Do not duplicate business rules across command services.
- Do not put database-heavy validation inside FluentValidation auto-validation.
- Do not change public API routes while migrating internals.
- Do not split every tiny use case into separate classes immediately; use command/query services first.
- Do not introduce MediatR, event sourcing, separate read databases, or projections until there is a real need.
- Do not let query services call command services.
- Do not let command services depend on controllers or HTTP-specific objects.
- Avoid returning tracked EF entities from queries. Keep `AsNoTracking` for read paths.
- Be careful with transaction boundaries in order, inventory, and sale workflows.
- Be careful with sale creation duplication between order delivery and manual invoice generation.
- Keep Slack/external notifications outside the database transaction or after successful commit where possible.

