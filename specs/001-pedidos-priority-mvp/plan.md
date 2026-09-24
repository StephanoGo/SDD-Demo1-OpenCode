# Plan 001 — Estrategia Técnica: Arquitectura en 3 Capas

## 1. Objetivo

Implementar el caso de uso de priorización y cola de empaquetamiento del spec `001-pedidos-priority-mvp` bajo una arquitectura estricta en 3 capas lógicas: Presentación, Servicio y Datos. Cero acoplamiento entre capas: la presentación conoce solo al servicio, y el servicio solo al repositorio.

## 2. Estructura de Solución

```
PedidosPriority/
│
├── Program.cs                     # Punto de entrada + registro de DI (único archivo a tocar)
├── PedidosPriority.sln            # NO modificar
│
├── Controllers/
│   └── EntregapedidosController.cs # Solo orquestación HTTP (GET cola, POST crear)
│
├── Views/
│   └── Entregapedidos/
│       ├── Index.cshtml            # Tabla de cola con badges "ALTA"/"BAJA"
│       └── _FormRegistro.cshtml    # Formulario: Cliente + Costo envío
│
├── Services/
│   ├── IOrderService.cs
│   └── OrderService.cs             # Reglas de negocio puras (prioridad, validaciones)
│
├── Data/
│   ├── IOrderRepository.cs
│   ├── OrderRepository.cs          # SqlCommand + CommandType.StoredProcedure
│   └── DbContext.cs (helper conexión)   # Contrato de conexión con SqlClient
│
├── Models/
│   └── Order.cs                    # Entidad de dominio
│
├── Views/_ViewImports, _Layout, wwwroot (CSS/JS vanilla)
│
└── SqlScripts/
    ├── 001_Create_PriorityLevels.sql
    ├── 001_Create_Orders.sql
    ├── 002_Seed_PriorityLevels.sql
    ├── 002_sp_InsertOrder.sql
    └── 002_sp_GetPackingQueue.sql
```

## 3. Responsabilidades por Capa

### 3.1 Presentación (`Controllers/`, `Views/`, `wwwroot/`)
- Recibe peticiones HTTP, valida modelos (`ModelState`), renderiza Razor, redirige tras POST (PRG).
- **Prohibido:** cálculo de prioridad, consultas SQL, comandos de base de datos.
- Única invocación: `IOrderService`.
- JS vanilla ES6 en `wwwroot/js/` para formateo de costo y confirmación, sin librerías externas.

### 3.2 Servicio (`Services/`)
- Implementa la regla pura: `PriorityLevel = ShippingCost > 0 ? 1 : 2`.
- Valida entradas de dominio (nombre requerido/máx 256, costo ≥ 0) antes de delegar.
- Orquesta llamadas a `IOrderRepository`.
- Nunca toca `SqlConnection`/`SqlCommand`.

### 3.3 Datos (`Data/`, `Models/`, `SqlScripts/`)
- `OrderRepository` implementa `IOrderRepository`, abre conexión, invoca los SP con `CommandType.StoredProcedure` y `SqlParameter` tipados, mapea a `Order`.
- Servicio y controlador no conocen la conexión ni los scripts.
- DDL y SPs viven versionados en `SqlScripts/`.

## 4. Acceso a Datos: Única Vía Permitida

- Cliente: `Microsoft.Data.SqlClient` (único paquete NuGet autorizado).
- Toda operación = Stored Procedure. Sin `CommandType.Text`, sin interpolación, sin concatenación de SQL.
- Ejemplo de la única forma válida:

```csharp
using (SqlCommand cmd = new SqlCommand("sp_InsertOrder", connection))
{
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.Parameters.Add(new SqlParameter("@CustomerName", order.CustomerName));
    cmd.Parameters.Add("@ShippingCost", SqlDbType.Decimal).Value = order.ShippingCost;
    SqlParameter outId = new SqlParameter("@OrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };
    cmd.Parameters.Add(outId);
    cmd.ExecuteNonQuery();
    return (int)outId.Value;
}
```

## 5. Inyección de Dependencias

En `Program.cs` (único archivo de configuración editable permitido):

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
```

Sin contenedor externo; uso de la inyección nativa de ASP.NET Core.

## 6. Base de Datos

- Motor: Microsoft SQL Server.
- Scripts DDL + SPs en `SqlScripts/` (nombres en inglés, comentarios en español).
- Tablas: `PriorityLevels` (lookup), `Orders`.
- SPs: `sp_InsertOrder`, `sp_GetPackingQueue`.
- Detalles de columnas, tipos y contratos en `spec.md` §5 y §7.

## 7. Estrategia de Pruebas

- Proyecto de pruebas **xUnit** (`PedidoPriority.Tests`).
- **Pruebas de regla de prioridad** (service/domain puro): `ShippingCost > 0 → 1`, `ShippingCost = 0 → 2`.
- **Pruebas de ordenamiento**: sobre una lista en memoria fake (`FakeOrderRepository`) validando el criterio `PriorityLevel ASC, RegistrationDate ASC`.
- Las pruebas de dominio no requieren SQL Server: usan `OrderService` con repositorio falso.
- Comandos de verificación: `dotnet build` y `dotnet test`.

## 8. Orden de Implementación Sugerido

1. Scaffold proyecto ASP.NET Core MVC + proyecto xUnit → `dotnet build` (rojo/verde inicial).
2. Scripts SQL (DDL, seed, SPs).
3. Modelo `Order`, `IOrderRepository`, `OrderRepository`.
4. `IOrderService`, `OrderService` + pruebas unitarias de dominio.
5. `EntregapedidosController` + vistas Razor (colas y formulario).
6. Tests de integración de ordenamiento con repositorio falso.
7. Verificación final: `dotnet build` + `dotnet test`.

## 9. Riesgos y Mitigaciones

| Riesgo | Mitigación |
|--------|------------|
| SQL inline en `.cs` | Regla de solo SP + revisión en checklist; AGENTS.md lo prohíbe. |
| Acoplamiento entre capas | Solo interfaces; controller→service, service→repository. |
| Ambigüedad de tiempo (UTC vs local) | Decidir y fijar en el SP (`SYSUTCDATETIME()`), documentar en spec. |
| Desempate en cola | `OrderId ASC` como tiebreaker estable en el `ORDER BY`. |