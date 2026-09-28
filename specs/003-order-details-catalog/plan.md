# Plan 003 — Selección de Cliente, Catálogo y Detalle de Orden

## 1. Objetivo

Convertir el formulario de registro en un flujo guiado (cliente + catálogo de productos) manteniendo el cálculo de
totales en cliente y añadiendo una pantalla dedicada de detalle de orden. Sin frameworks, 100% SPs, 3 capas lógicas.

## 2. Alcance

**Modelos y DTOs (crear/modificar):**
- `Models/OrderDetailRequest.cs` (nuevo): `ProductId`, `Quantity`, `UnitPrice`.
- `Models/CreateOrderRequest.cs` (modificar): + `List<OrderDetailRequest> Items`; se elimina `Subtotal`.
- `Models/ActiveCustomerDto.cs`, `Models/CategoryDto.cs`, `Models/ProductDto.cs` (nuevos).
- `Models/OrderDetailsViewModel.cs` (nuevo): cabecera (cliente, dirección, email, fecha, prioridad) + `Lines`.
- `Models/OrderDetailItemDto.cs` (nuevo): `ProductName`, `Quantity`, `UnitPrice`, `LineTotal`.

**Capa Data:**
- `Data/ICatalogRepository.cs` + `Data/CatalogRepository.cs` (nuevos): `GetActiveCustomers()`, `GetCategories()`,
  `GetProductsByCategory(int? categoryId)` — todos vía `SqlCommand` `CommandType.StoredProcedure` + `SqlParameter`.
- `Data/IOrderRepository.cs` (modificar): reemplazar `Insert(Order)` por `InsertWithDetails(Order, IEnumerable<OrderDetailRequest>)`
  (transacción con `sp_InsertOrder` + `sp_InsertOrderDetail`) y añadir `OrderDetailsViewModel GetOrderDetails(int orderId)`.

**Capa Service:**
- `Services/IOrderService.cs` + `Services/OrderService.cs` (modificar): `CreateOrder(int customerId, decimal shippingCost,
  IEnumerable<OrderDetailRequest> items)` con validaciones (≥1 ítem, Quantity ≥ 1, UnitPrice ≥ 0, shipping ≥ 0, customerId > 0),
  recálculo de `Subtotal`, `Total` y `PriorityLevel`; y `OrderDetailsViewModel GetOrderDetails(int orderId)`.

**Presentación:**
- `Controllers/PedidosController.cs` (modificar): `Create` adaptado al nuevo contrato, `Detalle(int id)` GET y endpoints
  JSON `Clientes()`, `Categorias()`, `Productos(int? categoriaId)`.
- `Views/Shared/_Layout.cshtml` (modificar): tercer tab activo "Ver detalles de la orden" con input `#detalle-id`.
- `Views/Pedidos/Index.cshtml` (reescribir): buscador de cliente + catálogo en cascada + tabla de ítems + totales en vivo.
- `Views/Pedidos/Detalle.cshtml` (nuevo): tarjeta con datos del cliente y desglose de líneas.
- `wwwroot/js/site.js` (modificar): lógica de selección cliente, catálogo, ítems y sincronización de campos ocultos.
- `wwwroot/css/site.css` (modificar): estilos para buscador, selects, tabla de ítems y sugerencias (paleta cálida del 002).

**Pruebas (PedidosPriority.Tests):**
- Reestructurar `FakeOrderRepository` para el nuevo contrato y ajustar los 8 tests existentes.
- Nuevos tests: subtotal por líneas, validaciones de cantidad/precio/sin ítems, prioridad ALTA/BAJA.

**No se tocan:** `.slnx`, `.csproj`, `Program.cs` (DI de nuevo `CatalogRepository` se registra según AGENTS.md),
ni SPs de SQL Server.

## 3. Estrategia Técnica

- **Persistencia transaccional (D1):** una conexión + `SqlTransaction`; `sp_InsertOrder` obtiene `@OrderId` (output)
  y por cada línea `sp_InsertOrderDetail`; `Commit` al final, `Rollback` ante `SqlException`.
- **Subtotal derivado (D2):** el servicio suma las líneas; el request ya no transporta `Subtotal`.
- **Endpoints JSON (D4):** `JsonResult` con `JsonSerializerOptions` camelCase; consultas por SP tipado.
- **Submit tradicional (D8):** formulario POST con `ValidateAntiForgeryToken`; JS genera antes del submit los campos
  ocultos `Items[n].ProductId|Quantity|UnitPrice` a partir de la tabla de ítems.
- **Detalle (D6):** `reader.NextResult()` mapea cabecera (rs1) y líneas (rs2) en el repositorio.
- **JS vanilla:** datos pre-cargados de clientes en `window.clientesData`; productos vía fetch JSON cuando cambia la
  categoría; construcción de filas con `createElement`/`textContent` (cero `innerHTML` con datos).

## 4. Detalle por Componente

| Componente | Responsabilidad |
|------------|-----------------|
| `CreateOrderRequest` + `OrderDetailRequest` | Contrato de entrada; `Items` liga con el Model Binder por nombres `Items[n].*` |
| `CatalogRepository` | Lectura catálogo/clientes vía los SPs del §3 del spec |
| `OrderRepository.InsertWithDetails` | Transacción cabecera+detalles; devuelve `OrderId` |
| `OrderRepository.GetOrderDetails` | Dos resultsets → `OrderDetailsViewModel` |
| `OrderService.CreateOrder` | Valida, calcula subtotal/total/prioridad, persiste |
| `PedidosController` | `Index`, `Create` (PRG a cola), `Detalle` (GET), endpoints JSON |
| `_Layout.cshtml` | Tercer tab activo con input de consulta directa |
| `Pedidos/Index.cshtml` + `site.js` | Búsqueda de cliente, cascada catálogo, tabla de ítems, totales en vivo, sync de campos ocultos |
| `Pedidos/Detalle.cshtml` | Tarjeta de orden con desglose y retorno a la cola |
| `site.css` | Estilos del nuevo formulario y detalle con paleta cálida |

## 5. Verificación

- `dotnet build` → 0 errores, 0 advertencias por tarea.
- `dotnet test` → suite completa en verde (8 migrados + nuevos de 003).
- Auditoría visual manual de `/`, `/entregapedidos` y `/Pedidos/Detalle/{id}`.
- Auditoría de dependencias: cero `cdn`/`npm`/Bootstrap/jQuery; iconos SVG inline.