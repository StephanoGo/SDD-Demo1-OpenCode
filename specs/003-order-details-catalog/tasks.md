# Tasks 003 — Checklist de Implementación

> Criterio de finalización: cada tarea compila limpiamente (`dotnet build`) y, al cierre, `dotnet test` en verde
> (8 tests migrados del 001 + nuevos de 003).

## T1 — Modelos, DTOs y Capa Data

- [x] T1.1 Crear `Models/OrderDetailRequest.cs` (ProductId, Quantity, UnitPrice) con `[Range]`.
- [x] T1.2 Extender `Models/CreateOrderRequest.cs`: + `Items` (List), eliminar `Subtotal`.
- [x] T1.3 Crear `Models/ActiveCustomerDto.cs`, `Models/CategoryDto.cs`, `Models/ProductDto.cs`.
- [x] T1.4 Crear `Models/OrderDetailItemDto.cs` (ProductName, Quantity, UnitPrice, LineTotal) y `Models/OrderDetailsViewModel.cs` (cabecera + Lines).
- [x] T1.5 Crear `Data/ICatalogRepository.cs` y `Data/CatalogRepository.cs`: `GetActiveCustomers`, `GetCategories`, `GetProductsByCategory(int?)` con `CommandType.StoredProcedure` + `SqlParameter`.
- [x] T1.6 Modificar `Data/IOrderRepository.cs` y `Data/OrderRepository.cs`: `InsertWithDetails(Order, IEnumerable<OrderDetailRequest>)` con transacción (`sp_InsertOrder` + `sp_InsertOrderDetail` por línea) y `GetOrderDetails(int orderId)` con `NextResult()`.
- [x] T1.7 Registro de dependencia en `Program.cs` para `ICatalogRepository`/`CatalogRepository`.
- [x] T1.8 Verificación: `dotnet build` sin errores.

## T2 — Capa Service y Pruebas Unitarias

- [x] T2.1 Ajustar `Services/IOrderService.cs` y `Services/OrderService.cs`: `CreateOrder(int customerId, decimal shippingCost, IEnumerable<OrderDetailRequest> items)`.
- [x] T2.2 Validaciones: ≥1 ítem, cantidad ≥ 1, precio ≥ 0, envío ≥ 0, cliente > 0.
- [x] T2.3 Cálculo: Subtotal = Σ(Quantity × UnitPrice), Total = Subtotal + ShippingCost, Prioridad (envío > 0 → ALTA / si no → BAJA).
- [x] T2.4 Método `OrderDetailsViewModel GetOrderDetails(int orderId)`.
- [x] T2.5 Actualizar `FakeOrderRepository` al nuevo contrato y migrar los 8 tests existentes.
- [x] T2.6 Nuevos tests xUnit: subtotal por líneas, validaciones (sin ítems, cantidad 0, precio negativo), prioridad ALTA/BAJA, mapeo del detalle (2 resultsets).
- [x] T2.7 Verificación: `dotnet build` y `dotnet test` en verde.

## T3 — Controladores y Endpoints

- [x] T3.1 Adaptar `PedidosController.Create` al nuevo contrato (PRG a `/entregapedidos`, `ValidateAntiForgeryToken`, manejo de errores de validación).
- [x] T3.2 Endpoint JSON `Clientes()` → `sp_GetActiveCustomers`.
- [x] T3.3 Endpoint JSON `Categorias()` → `sp_GetCategories`.
- [x] T3.4 Endpoint JSON `Productos(int? categoriaId)` → `sp_GetProductsByCategory`.
- [x] T3.5 Acción GET `Detalle(int id)` → `GetOrderDetails` + vista.
- [x] T3.6 Verificación: `dotnet build` sin errores.

## T4 — Vista Detalle de Orden (`/Pedidos/Detalle/{id}`)

- [x] T4.1 Vista `Detalle.cshtml` con tarjeta de cliente (nombre, teléfono, dirección, email, fecha de registro).
- [x] T4.2 Badge de prioridad (ALTA/BAJA) con paleta cálida del 002.
- [x] T4.3 Tabla de desglose: Producto, Cantidad, Precio Unitario, Subtotal de línea; fila de totales (Subtotal, Envío, Total).
- [x] T4.4 Estado "sin resultados" con enlace "← Volver a la cola" (`/entregapedidos`).
- [x] T4.5 Verificación: `dotnet build` sin errores.

## T5 — Refactor formulario y JS (`site.js`)

- [x] T5.1 `_Layout.cshtml`: tercer tab activo "Ver detalles de la orden" con input `#detalle-id` + navegación JS (Enter/clic, solo id ≥ 1).
- [x] T5.2 `Pedidos/Index.cshtml`: buscador de cliente con `window.clientesData` precargado y campo oculto `CustomerId`.
- [x] T5.3 Cascada catálogo: `#categoria-select` → `Pedidos/Categorias`, `#producto-select` → `Pedidos/Productos?categoriaId=` (fetch JSON).
- [x] T5.4 Tabla de ítems: agregar/quitar líneas (Producto, Cantidad, PU, Subtotal línea).
- [x] T5.5 Cálculo reactivo: `#subtotal-general` = Σ líneas; `#TotalEstimado` = Subtotal + Envío; presets Gratis 0 / Express 15.00.
- [x] T5.6 Sincronización de campos ocultos `Items[n].ProductId|Quantity|UnitPrice` antes del submit (Model Binder + antiforgery).
- [x] T5.7 Estilos CSS (site.css): buscador, sugerencias, selects, tabla de ítems — paleta cálida del 002.
- [x] T5.8 Verificación: `dotnet build` sin errores.

## T6 — Verificación Integral y Cierre

- [x] T6.1 `dotnet build` → 0 errores, 0 advertencias.
- [x] T6.2 `dotnet test` → suite completa en verde.
- [x] T6.3 Auditoría visual: `/`, `/entregapedidos`, `/Pedidos/Detalle/{id}` y tab "Ver detalles de la orden".
- [x] T6.4 Auditoría de dependencias → cero frameworks/librerías externas.
- [x] T6.5 Marcar todos los checklists como completados.

## Contratos Imprescindibles (no negociables)

1. Cero frameworks/ORMs externos en HTML/CSS/JS; iconos SVG inline.
2. Toda operación de BD exclusivamente vía SPs tipados (sin SQL inline en `.cs`).
3. UI en español; identificadores técnicos en inglés.
4. Persistencia transaccional: si falla una línea, no se persiste la orden.
5. Subtotal siempre derivado en el servicio; nunca del cliente.
6. Presets fijos: Gratis = `0`, Express = `15.00`; prioridad ALTA solo si envío > 0.
7. Tras `Create` → redirect a `/entregapedidos`.