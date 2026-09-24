# Tasks 001 — Checklist de Implementación

> Criterio de finalización: cada tarea termina compilando limpiamente (`dotnet build`) y, donde aplique, pasando `dotnet test`. Referencia de contratos: `spec.md`, arquitectura: `plan.md`.

## Fase 0 — Preparación

- [x] T0.1 Crear solución `PedidosPriority` y proyecto web MVC (ASP.NET Core, .NET 10).
- [x] T0.2 Crear estructura de carpetas: `Controllers/`, `Views/`, `Services/`, `Data/`, `Models/`, `wwwroot/`, `SqlScripts/`.
- [x] T0.3 Agregar único paquete NuGet permitido: `Microsoft.Data.SqlClient`.
- [x] T0.4 Verificación: `dotnet build` sin errores.

## Fase 1 — Base de Datos (SqlScripts/)

- [x] T1.1 `001_Create_PriorityLevels.sql`: tabla lookup `PriorityLevels` + seed `(1,'ALTA')`, `(2,'BAJA')`.
- [x] T1.2 `001_Create_Orders.sql` (+`Customers`): normalización con FK, `CK_Orders_ShippingCost_NonNegative` e índice `(PriorityLevel ASC, RegistrationDate ASC)`.
- [x] T1.3 `dbo.sp_InsertOrder`: valida costos ≥ 0, calcula `Total` y prioridad, inserta y expone `@OrderId OUTPUT`.
- [x] T1.4 `dbo.sp_GetPackingQueue`: proyección `OrderId, CustomerFullName, CustomerPhone, DeliveryAddress, Subtotal, ShippingCost, Total, PriorityLevel, PriorityName, RegistrationDate` con `ORDER BY` definido.
- [x] T1.5 Ejecutar scripts contra SQL Server y verificar SPs con un insert de prueba. Sin SQL inline en C#.

## Fase 2 — Modelo y Repositorio (Data/, Models/)

- [x] T2.1 `Models/Order.cs`: entidad normalizada (`OrderId`, `CustomerId`, `Subtotal`, `ShippingCost`, `Total`, `PriorityLevel`, `RegistrationDate` + proyección de la cola).
- [x] T2.2 `Data/IOrderRepository.cs`: `Insert(Order)` y `GetPackingQueue()`.
- [x] T2.3 `Data/OrderRepository.cs`: `SqlCommand` + `CommandType.StoredProcedure` + `SqlParameter` tipados para ambos SPs.
- [x] T2.4 Cadena de conexión en `appsettings.json` (`ConnectionStrings:PedidosDb`) inyectada vía `IConfiguration`.
- [x] T2.5 Verificación: `dotnet build` sin errores.

## Fase 3 — Servicio (Services/)

- [x] T3.1 `IOrderService.cs`: `CreateOrder(int customerId, decimal subtotal, decimal shippingCost)` y `GetPackingQueue()`.
- [x] T3.2 `OrderService.cs`: validaciones de dominio y regla pura de prioridad.
- [x] T3.3 Registrar DI en `Program.cs`: `IOrderService` y `IOrderRepository` registrados.
- [x] T3.4 Verificación: `dotnet build` sin errores.

## Fase 4 — Pruebas Unitarias (xUnit)

- [x] T4.1 Crear proyecto de pruebas `PedidosPriority.Tests` y agregarlo a la solución.
- [x] T4.2 Prueba regla prioridad: `ShippingCost > 0` → nivel 1 (ALTA).
- [x] T4.3 Prueba regla prioridad: `ShippingCost = 0` → nivel 2 (BAJA).
- [x] T4.4 Prueba ordenamiento de cola: `FakeOrderRepository` con datos mixtos → ALTA primero + `RegistrationDate ASC`.
- [x] T4.5 Prueba validación: `ShippingCost < 0` (además de `Subtotal <= 0` y `CustomerId <= 0`) rechazados en servicio.
- [x] T4.6 Verificación: `dotnet test` en verde (8/8 pruebas).

## Fase 5 — Presentación (Controllers/, Views/, wwwroot/)

- [x] T5.1 `EntregapedidosController`: `GET Index` (cola vía servicio) y `POST Create` con PRG.
- [x] T5.2 Vista `Index.cshtml`: tabla con Turno, Cliente, Teléfono, Dirección, Subtotal, Costo de envío, Total, badges "ALTA"/"BAJA", "Envío Gratis"/"Envío Express".
- [x] T5.3 Formulario de registro: `CustomerId` (entero > 0), `Subtotal` (> 0) y `ShippingCost` (≥ 0), validación de modelo (server) + repoblamiento en errores.
- [x] T5.4 CSS limpio y JS vanilla en `wwwroot/` (sin librerías externas: se eliminó referencia a Bootstrap/jQuery del layout).
- [x] T5.5 Verificación: `dotnet build` sin errores. Ruta por defecto → `/Entregapedidos` y enlace en `_Layout.cshtml`.

## Fase 6 — Verificación y Cierre

- [x] T6.1 Ejecutar `dotnet build` → sin errores ni warnings bloqueantes.
- [x] T6.2 Ejecutar `dotnet test` → todas las pruebas en verde (8/8).
- [x] T6.3 Auditoría final: grep de SQL inline/concatenación en `.cs` → cero hallazgos; solo `CommandType.StoredProcedure`.
- [x] T6.4 Marcar todos los checklists como completados.

## Contratos Imprescindibles (no negociables)

1. Identificadores técnicos en inglés (decisión D1 de `spec.md`).
2. UI en español ("ALTA", "BAJA", "Envío Gratis", "Envío Express").
3. Todas las operaciones de BD vía Stored Procedures (`CommandType.StoredProcedure`) — prohibido SQL inline.
4. Sin ORMs, sin frameworks frontend, sin tocar `.sln`/`.csproj` salvo lo autorizado.
5. El nivel de prioridad nunca se recibe del usuario: lo calcula el backend/BD.