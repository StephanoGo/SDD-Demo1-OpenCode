# Spec 003 — Selección de Cliente, Catálogo de Productos y Detalle de Orden

## 1. Visión General

Ampliación funcional del spec 001 sobre la base visual del 002. Reemplaza el ingreso manual de `CustomerId`/`Subtotal` por:
un **buscador de cliente**, un **catálogo de categorías → productos** que arma líneas de pedido y calcula el subtotal de
forma dinámica en cliente, y una **pantalla dedicada de detalle de orden**.

**No altera** reglas de negocio del 001 (prioridad ALTA si envío con costo > 0, cola ordenada por prioridad/fecha).
**No introduce** frameworks ni ORMs: CSS/JS vanilla, 100% Stored Procedures, arquitectura de 3 capas, UI en español.

## 2. Decisiones Registradas

| # | Decisión | Justificación |
|---|----------|---------------|
| D1 | La persistencia de cabecera + detalles es **transaccional en el repositorio**: una única `SqlConnection` + `SqlTransaction` que ejecuta `sp_InsertOrder` y luego `sp_InsertOrderDetail` por línea; rollback ante cualquier error. No se crean SPs nuevos. | Confirmado por el usuario; respeta "100% SPs" reutilizando los existentes. |
| D2 | El **subtotal no se confía del cliente**: la capa Service lo recalcula como Σ(Cantidad × PrecioUnitario) desde las líneas recibidas. Total y Prioridad se calculan en el servicio (envío > 0 → ALTA). | Evita totales manipulados; mantiene la regla vigente de prioridad. |
| D3 | El tercer tab de la píldora pasa a **"Ver detalles de la orden"** con **input de consulta directa** por `OrderId` → navega a `/Pedidos/Detalle/{id}`. | Confirmado por el usuario (sin SP extra, stateless). |
| D4 | Endpoints JSON (`Clientes()`, `Categorias()`, `Productos(int? categoriaId)`) en `PedidosController`, serialización camelCase, consultas vía SP con `SqlParameter` tipados. | Un solo controlador; sin configurar rutas adicionales. |
| D5 | `CreateOrderRequest` se **extiende** con `List<OrderDetailRequest> Items` (ProductId, Quantity, UnitPrice); se elimina `Subtotal` del request (derivado en servidor). Los tests existentes se migran a la nueva firma del servicio. | El subtotal ya no es entrada del usuario. |
| D6 | El detalle se lee con `sp_GetOrderDetailsByOrderId` y `reader.NextResult()` (resultsets 1 y 2) → `OrderDetailsViewModel`. | El SP ya devuelve cabecera + líneas en 2 resultsets. |
| D7 | Tras `Create` exitoso, **redirect a la cola** (`/entregapedidos`); el detalle se alcanza desde el tab del navbar. | Confirmado por el usuario. |
| D8 | El submit es **tradicional** (POST + `ValidateAntiForgeryToken` + Model Binder): `CustomerId` viaja en campo oculto y los ítems se sincronizan como campos ocultos `Items[0].ProductId`, `Items[0].Quantity`, `Items[0].UnitPrice`. | Confirmado por el usuario; sin fetch POST, compatible con el binder. |
| D9 | Moneda PEN con prefijo **"S/"** solo en presentación (formato local `es-PE`, 2 decimales). | Consistente con specs 001/002. |
| D10 | UI en **español**; identificadores técnicos C#/SQL/JS en **inglés**. | AGENTS.md §4 y constitution. |

## 3. Contratos de Base de Datos (SPs existentes en SQL Server)

| SP | Parámetros | Retorno |
|----|-----------|---------|
| `dbo.sp_GetActiveCustomers` | — | Resultset: `CustomerId`, `FirstName`, `LastName`, `FullName`, `Phone`, `DeliveryAddress`, `Email` |
| `dbo.sp_GetCategories` | — | Resultset: `CategoryId`, `Name`, `Description` (tabla `Categories` no tiene `IsActive`) |
| `dbo.sp_GetProductsByCategory` | `@CategoryId INT = NULL` | Resultset: `ProductId`, `CategoryId`, `CategoryName`, `ProductName`, `UnitPrice`, `Stock` (solo `IsActive = 1` y `Stock > 0`; NULL devuelve todos) |
| `dbo.sp_InsertOrder` | `@CustomerId INT`, `@Subtotal DECIMAL`, `@ShippingCost DECIMAL` | `@OrderId INT OUTPUT` |
| `dbo.sp_InsertOrderDetail` | `@OrderId INT`, `@ProductId INT`, `@Quantity INT`, `@UnitPrice DECIMAL` | `@OrderDetailId INT OUTPUT` (se lee y descarta) |
| `dbo.sp_GetOrderDetailsByOrderId` | `@OrderId INT` | **2 resultsets**: (1) cabecera: datos del cliente, `RegistrationDate`, `PriorityLevels.Id`/nombre; (2) líneas: producto, `Quantity`, `UnitPrice`, `LineTotal` |

## 4. Registro de Pedido (`/`)

- **Selector / Búsqueda de Cliente**: los clientes activos se precargan en el cliente (`window.clientesData` desde
  `sp_GetActiveCustomers` vía `Pedidos/Clientes`). El input filtra en memoria por ID o Nombre/Apellido; al elegir una
  opción se actualiza el campo oculto `CustomerId` y se muestra el cliente seleccionado.
- **Catálogo en dos pasos**: `Pedidos/Categorias` puebla el `<select>` de categorías; al cambiar, `Pedidos/Productos?categoriaId=`
  carga los productos disponibles (Stock > 0). Seleccionar producto + cantidad agrega una línea al pedido.
- **Tabla de ítems dinámica (JS vanilla)**: columnas Producto, Cantidad, Precio Unitario, Subtotal de línea y botón
  "quitar". Cero librerías.
- **Cálculo reactivo**: el **Subtotal general = Σ de los subtotales de línea**. El **Total Estimado = Subtotal +
  Costo de Envío** recalculado en vivo; los presets se mantienen fijos (**Gratis S/ 0** / **Express S/ 15.00**).
- **Persistencia**: al enviar, la capa Service recalcula/valida y el repositorio persiste cabecera (orden) y detalles en
  una transacción (D1). SI la línea no puede validarse o hay ítems inválidos, se muestra error de validación y no se insiste.

## 5. Pantalla Detalle de Orden (`/Pedidos/Detalle/{id}`)

- Ruta **GET** que consulta `dbo.sp_GetOrderDetailsByOrderId` con `@OrderId` tipado (`SqlDbType.Int`).
- Tarjeta con: nombre completo del cliente, teléfono, dirección, `Email`, fecha de registro y **estado de prioridad**
  (badge ALTA/BAJA, paleta cálida del 002).
- Tabla de desglose: `ProductName`, `Quantity`, `UnitPrice`, `LineTotal`; fila de totales: Subtotal, Envío y **Total**.
- Enlace de retorno: "← Volver a la cola" (`/entregapedidos`).
- Si el `OrderId` no existe o no hay resultados: mensaje de sin resultados con retorno a la cola.

## 6. Navegación del Header

- Píldora de 3 tabs: "Registrar Pedido" (`/`), "Pedidos en Cola" (`/entregapedidos`) y **"Ver detalles de la orden"**.
- El tercer tab muestra un **input numérico de consulta directa** (`#detalle-id`); al presionar Enter o el botón "Ver",
  JS navega a `/Pedidos/Detalle/{id}`. Valida que sea un entero `≥ 1`.
- Se conservan los iconos SVG del 002 (más `+`, paquete y camión) y el footer global.

## 7. Contrato JS (`wwwroot/js/site.js`)

Selectores y responsabilidades (todo vanilla, sin librerías):

| Selector | Rol |
|----------|-----|
| `#buscar-cliente` | Filtra `window.clientesData` por ID o nombre/apellido y muestra sugerencias |
| `#customer-id-hidden` (name=`CustomerId`) | Campo oculto sincronizado con el cliente elegido |
| `#categoria-select` | Carga y administra los productos por categoría (cascada) |
| `#producto-select` | Selector de producto del catálogo |
| `#cantidad-input` | Cantidad a agregar por línea |
| `#items-table tbody` | Tabla de ítems agregados; botón "quitar" por fila |
| `.shipping-preset`, `#Subtotal`, `#ShippingCost` | Presets de envío y totales (lógica existente del 002) |
| `#subtotal-general`, `#TotalEstimado` | Subtotal Σ líneas y Total Estimado en vivo, formato "S/ N,N0" |
| `#detalle-id` | Input de consulta del tab "Ver detalles de la orden" |
| `Items[0].ProductId`… (ocultos) | Sincronizados por JS desde la tabla de ítems antes del submit |

Funciones: `filterCustomers`, `selectCustomer`, `loadProducts`, `addItem`, `removeItem`, `recalculateTotal`, `syncHiddenItems`, `openOrderDetail`.
- Cero `innerHTML` con datos no sanitizados (construcción de nodos con `textContent`).

## 8. Criterios de Aceptación

1. `Pedidos/Clientes`, `Pedidos/Categorias` y `Pedidos/Productos` responden JSON camelCase desde SPs tipados.
2. El registro `/` permite buscar cliente, elegir categoría→producto, agregar/quitar ítems y ve Subtotal + TOTAL ESTIMADO en vivo.
3. El envío con costo (Express S/ 15.00) sigue asignando prioridad **ALTA**; Gratis (S/ 0) → **BAJA**.
4. `Create` persiste cabecera y detalles **transaccionalmente**; ante error de validación no persiste nada y se muestra el error.
5. `/Pedidos/Detalle/{id}` muestra la tarjeta de cliente, fecha, prioridad, desglose de líneas con importes y enlace de retorno.
6. El tab "Ver detalles de la orden" navega a `/Pedidos/Detalle/{id}` desde el input; id inválido no navega.
7. `Create` exitoso redirige a `/entregapedidos`.
8. **Cero** frameworks/librerías externas; UI en español; identificadores técnicos en inglés.
9. `dotnet build` → 0 errores; `dotnet test` → suite completa en verde.