# Spec 001 — Sistema de Priorización y Cola de Empaquetamiento de Pedidos (MVP)

## 1. Visión General

Caso de uso inicial del sistema: registrar pedidos y presentar una cola de empaquetamiento (`/entregapedidos`) donde los pedidos con costo de envío extra se empaquetan primero y los de envío gratis al final.

Este documento es la **única fuente de verdad** sobre los contratos de datos y el comportamiento del caso de uso. Ningún código, endpoint, propiedad o vista puede implementarse sin estar documentado aquí.

## 2. Decisiones Registradas

| # | Decisión | Justificación |
|---|----------|---------------|
| D1 | Los identificadores técnicos (tablas, columnas, SPs, clases, métodos) se escriben en **inglés** (ej. `Order`, `ShippingCost`, `sp_InsertOrder`). | Resolución explícita del usuario ante la contradicción entre `AGENTS.md` §4 (inglés) y `docs/constitution.md` §6 (español). AGENTS.md manda para el caso de uso. |
| D2 | El texto visible en la UI (labels, headings, badges) se escribe en **español** ("ALTA", "BAJA", "Envío Gratis"). | `Agents.md` §4 y `constitution.md` §6. |
| D3 | La documentación técnica (spec/plan/tasks) se escribe en **español neutro**. | `AGENTS.md` §4. |

## 3. Regla de Negocio: Priorización

Dado un pedido con un costo de envío `ShippingCost`:

- Si `ShippingCost > 0` → costo de envío **extra** → `PriorityLevel = 1` → prioridad **ALTA**.
- Si `ShippingCost = 0` → envío **gratis** → `PriorityLevel = 2` → prioridad **BAJA**.

Sobre `ShippingCost`:

- El valor es no negativo. Valores `< 0` se rechazan (violación de restricción `CK_Orders_ShippingCost_NonNegative`).
- `ShippingCost` es un valor decimal fijo (sin impuestos ni IVA en este MVP). Cálculo de impuestos está fuera de alcance.
- El nivel de prioridad **no es ingresado por el usuario**: lo calcula el backend/BD de forma determinista a partir de `ShippingCost`.

## 4. Alcance (In / Out)

**In (MVP):**
- Registro de un pedido para un cliente existente (`CustomerId`) con `Subtotal` y `ShippingCost`.
- Cálculo determinista del total, del nivel de prioridad y de la fecha de registro.
- Consulta de la cola de empaquetamiento ordenada.
- Vista `Entregapedidos` para empaquetadores.

**Out (fuera de alcance):**
- Autenticación/autorización, catalogación de clientes, catálogo de productos, gestión de inventario, estados avanzados del pedido, cálculos de impuestos, reporte histórico, actualización de pedidos.

## 5. Modelo de Datos (SQL Server)

Base de datos: **PedidosDb**. Cadena de conexión (configuración `appsettings.json`):

```
Server=localhost;Database=PedidosDb;User Id=sa;Password=<secreto>;TrustServerCertificate=True;
```

### 5.1 Tabla `PriorityLevels` (lookup read-only)

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `Id` | `tinyint` | `PRIMARY KEY` | Nivel (1 = ALTA, 2 = BAJA) |
| `Name` | `nvarchar(50)` | `NOT NULL UNIQUE` | Nombre de la prioridad (`ALTA`, `BAJA`) |

Seed inmutable: `(1, 'ALTA')`, `(2, 'BAJA')`.

### 5.2 Tabla `Customers` (catálogo de clientes)

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `CustomerId` | `int` | `IDENTITY(1,1) PRIMARY KEY` | Identificador del cliente |
| `FirstName` | `nvarchar(128)` | `NOT NULL` | Primer nombre |
| `LastName` | `nvarchar(128)` | `NOT NULL` | Apellido |
| `Phone` | `nvarchar(32)` | `NOT NULL` | Teléfono de contacto |
| `DeliveryAddress` | `nvarchar(256)` | `NOT NULL` | Dirección de entrega |

> Gestión CRUD de clientes está fuera de alcance del MVP; la tabla ya existe y se consulta de forma read-only vía los SPs de pedidos.

### 5.3 Tabla `Orders` (normalizada)

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `OrderId` | `int` | `IDENTITY(1,1) PRIMARY KEY` | Identificador del pedido |
| `CustomerId` | `int` | `NOT NULL`, `FOREIGN KEY → Customers(CustomerId)` | Cliente del pedido |
| `Subtotal` | `decimal(10,2)` | `NOT NULL`, `CHECK (Subtotal >= 0)` | Importe sin envío |
| `ShippingCost` | `decimal(10,2)` | `NOT NULL`, `CHECK (ShippingCost >= 0)` | Costo de envío: 0 = gratis, >0 = extra |
| `Total` | `decimal(10,2)` | `NOT NULL`, `CHECK (Total >= 0)` | Importe total = `Subtotal + ShippingCost` |
| `PriorityLevel` | `tinyint` | `NOT NULL`, `FOREIGN KEY → PriorityLevels(Id)` | Nivel calculado: 1 = ALTA, 2 = BAJA |
| `RegistrationDate` | `datetime2` | `NOT NULL` | Marca de tiempo de registro (server time) |

Restricciones derivadas:

- `CK_Orders_ShippingCost_NonNegative`: `ShippingCost >= 0`.
- `CK_Orders_Subtotal_NonNegative`: `Subtotal >= 0`.
- `CK_Orders_Total_NonNegative`: `Total >= 0`.
- `FK_Orders_Customer`: `CustomerId` referencia a `Customers(CustomerId)`.
- `FK_Orders_PriorityLevel`: `PriorityLevel` referencia a `PriorityLevels(Id)`.
- `IX_Orders_QueueOrder`: índice sobre `(PriorityLevel ASC, RegistrationDate ASC)` para optimizar la consulta de la cola.

## 6. Endpoint / Flujo de Presentación

Flujo en dos pantallas separadas: registro de pedido en `/` (`Pedidos`) y cola de empaquetamiento en `/entregapedidos`.

### 6.1 Cola de empaquetamiento — `GET /Entregapedidos`

Contiene **únicamente** la tabla "PEDIDOS EN COLA". Si no hay pedidos, muestra el mensaje: **"No hay pedidos pendientes en la cola"**. Orden determinista:

```
ORDER BY PriorityLevel ASC, RegistrationDate ASC
```

Es decir: primero los pedidos ALTA (1) y, dentro de cada nivel, los registrados primero (fecha más antigua primero). Para un mismo timestamp se desempata por `OrderId ASC` (estable).

Columnas visibles en la tabla:

| Encabezado (UI) | Fuente |
|-----------------|--------|
| Turno | `OrderId` |
| Cliente | `CustomerFullName` (= `FirstName + ' ' + LastName`) |
| Teléfono | `CustomerPhone` |
| Dirección | `DeliveryAddress` |
| Subtotal | `Subtotal` (format `decimal`) |
| Costo envío | `ShippingCost` (format `decimal`) |
| Total | `Total` (format `decimal`) |
| Prioridad | badge derivado de `PriorityLevel`/`PriorityName` → "ALTA" (rojo) o "BAJA" (verde) |
| Envío | `ShippingCost > 0` → "Envío Express" / `ShippingCost = 0` → "Envío Gratis" |

### 6.2 Registro de pedido — `GET /` y `POST /Pedidos/Create`

Vista de registro independiente (`Pedidos/Index`) con el formulario de alta.

- Inputs: `CustomerId` (entero, debe existir), `Subtotal` (decimal > 0), `ShippingCost` (decimal ≥ 0).
- Validación de modelo (`ModelState`) en la capa de presentación: requeridos y rangos definidos.
- El servidor calcula `Total` (`Subtotal + ShippingCost`), `PriorityLevel` y `RegistrationDate`.
- Al hacer clic en "Agregar a la cola", se guarda el pedido y se redirige a `/Entregapedidos` (`PRG`) para observar cómo entra en su posición.

## 7. Contratos de Stored Procedures

Todos los accesos a datos se ejecutan exclusivamente vía `SqlCommand` con `CommandType.StoredProcedure` y `SqlParameter` tipados. **Prohibido SQL inline o concatenación** en C#.

### 7.1 `dbo.sp_InsertOrder`

Inserta un pedido, calcula `Total`, el nivel de prioridad y la fecha de registro dentro de la base de datos.

**Entrada:**

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `@CustomerId` | `int` | Cliente existente en `Customers` |
| `@Subtotal` | `decimal(10,2)` | Importe sin envío (≥ 0) |
| `@ShippingCost` | `decimal(10,2)` | Costo de envío (≥ 0) |

**Salida:**

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `@OrderId` | `int` OUTPUT | Nuevo `OrderId` insertado |

**Comportamiento:**
1. Valida `@Subtotal` y `@ShippingCost` no negativos; si violan, lanza error y no inserta.
2. Calcula `Total = @Subtotal + @ShippingCost`.
3. Calcula `PriorityLevel = 1` si `@ShippingCost > 0`, `2` si `= 0`.
4. Inserta en `Orders` con `RegistrationDate` de servidor (decisión T-BD: `SYSUTCDATETIME()` o `GETDATE()`).
5. Devuelve `@OrderId`.

### 7.2 `dbo.sp_GetPackingQueue`

Devuelve la cola de empaquetamiento.

**Sin parámetros.**

**Salida:** proyección por fila:

| Columna | Tipo | Descripción |
|---------|------|-------------|
| `OrderId` | `int` | Turno |
| `CustomerFullName` | `nvarchar(257)` | `FirstName + ' ' + LastName` |
| `CustomerPhone` | `nvarchar(32)` | Teléfono |
| `DeliveryAddress` | `nvarchar(256)` | Dirección de entrega |
| `Subtotal` | `decimal(10,2)` | Importe sin envío |
| `ShippingCost` | `decimal(10,2)` | Costo de envío |
| `Total` | `decimal(10,2)` | Importe total |
| `PriorityLevel` | `tinyint` | Nivel |
| `PriorityName` | `nvarchar(50)` | Nombre de prioridad |
| `RegistrationDate` | `datetime2` | Fecha de registro |

Orden aplicado:

```
ORDER BY PriorityLevel ASC, RegistrationDate ASC, OrderId ASC
```

Uso en C#: `CommandType.StoredProcedure`, sin parámetros; leer filas con `SqlDataReader`.

## 8. Contratos de Código (Capa de Aplicación)

### 8.1 Modelo de dominio

```csharp
public sealed class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public byte PriorityLevel { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? PriorityName { get; set; }
}
```

> Nota: los contratos aquí son de referencia para el plan, no se implementan en esta tarea de especificación.

### 8.2 Repositorio

```csharp
public interface IOrderRepository
{
    int Insert(Order order);                 // invoca dbo.sp_InsertOrder
    IEnumerable<Order> GetPackingQueue();    // invoca dbo.sp_GetPackingQueue
}
```

### 8.3 Servicio

```csharp
public interface IOrderService
{
    int CreateOrder(int customerId, decimal subtotal, decimal shippingCost);
    IEnumerable<Order> GetPackingQueue();
}
```

`OrderService` orquesta repositorio y aplica reglas de negocio puras (validaciones de dominio) sin tocar la base de datos.

## 9. Criterios de Aceptación

1. Registrar un pedido con `ShippingCost = 15.50` produce `PriorityLevel = 1` (ALTA) y `Total = Subtotal + 15.50`.
2. Registrar un pedido con `ShippingCost = 0` produce `PriorityLevel = 2` (BAJA).
3. La cola `sp_GetPackingQueue` devuelve primero todos los ALTA y después los BAJA; dentro de cada nivel, por orden de registro.
4. `ShippingCost < 0` o `Subtotal < 0` son rechazados tanto en validación de UI como en el SP (`dbo.sp_InsertOrder` falla).
5. `CustomerId` inexistente es rechazado por la FK/el SP.
6. No existe SQL inline ni concatenación de cadenas en ningún archivo `.cs`.
7. Todas las operaciones de BD pasan por Stored Procedures (`dbo.sp_InsertOrder`, `dbo.sp_GetPackingQueue`).