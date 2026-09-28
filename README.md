# Documentación del Demo Sistema PedidosPriority (SDD)

Este documento registra la trazabilidad completa, la arquitectura técnica y el historial de prompts utilizados junto con el asistente de inteligencia artificial (OpenCode) para la construcción del proyecto **PedidosPriority** bajo la metodología **Spec-Driven Development (SDD)**.

---

## 1. Visión General del Proyecto

Sistema MVP para registrar pedidos y organizar una cola de empaquetamiento determinista (`/entregapedidos`):
- **Regla de Priorización:**
  - Costo de envío extra (`ShippingCost > 0`) $\rightarrow$ **Prioridad 1 (ALTA)** (Envío Express).
  - Envío gratis (`ShippingCost = 0`) $\rightarrow$ **Prioridad 2 (BAJA)** (Envío Gratis).
- **Criterio de Ordenamiento de Cola:**
  - `PriorityLevel ASC` (primero todos los envíos ALTA antes que los BAJA).
  - `RegistrationDate ASC` (orden de llegada FIFO para pedidos con igual nivel de prioridad).
  - `OrderId ASC` (criterio de desempate determinista).

---

## 2. Stack Tecnológico y Restricciones Arquitectónicas

- **Framework Web:** ASP.NET Core MVC (.NET 10).
- **Base de Datos:** Microsoft SQL Server 2022.
- **Acceso a Datos:** `Microsoft.Data.SqlClient` (único paquete NuGet permitido).
- **Restricción de Seguridad:** Prohibido el SQL inline o concatenado; 100% de operaciones ejecutadas mediante Stored Procedures con parámetros fuertemente tipados.
- **Frontend:** HTML5, CSS nativo y JavaScript vanilla (cero frameworks como Bootstrap, jQuery o Tailwind).
- **Pruebas Automatizadas:** xUnit con repositorio simulado en memoria (`FakeOrderRepository`).
- **Convención de Idioma:** Código fuente, identificadores e interfaces en inglés; documentación e interfaz de usuario (UI) en español.

---

## 3. Modelo de Datos y Procedimientos Almacenados (SQL Server)

### 3.1 Tablas Implementadas
- **`PriorityLevels`:** Catálogo de prioridades (`1 = ALTA`, `2 = BAJA`).
- **`Customers`:** Maestro de clientes con datos de contacto (`CustomerId`, `FirstName`, `LastName`, `Phone`, `DeliveryAddress`, `Reference`, `Email`, `IsActive`).
- **`Categories` & `Products`:** Catálogo base de artículos para simulación de compras.
- **`Orders`:** Cabecera de pedidos con importes y prioridad calculada (`OrderId`, `CustomerId`, `Subtotal`, `ShippingCost`, `Total`, `PriorityLevel`, `RegistrationDate`).
- **`OrderDetails`:** Líneas del pedido asociadas al producto.

### 3.2 Procedimientos Almacenados Oficiales
- **`dbo.sp_InsertOrder`:**
  - Parámetros: `@CustomerId INT`, `@Subtotal DECIMAL(10,2)`, `@ShippingCost DECIMAL(10,2)`, `@OrderId INT OUTPUT`.
  - Valida cliente activo y costo no negativo.
  - Asigna deterministamente el nivel de prioridad y calcula el total (`Subtotal + ShippingCost`).
  - Registra la fecha con `SYSUTCDATETIME()`.
- **`dbo.sp_GetPackingQueue`:**
  - Proyecta la cola de pedidos realizando join con `Customers` y `PriorityLevels`.
  - Ordena de forma inmutable: `PriorityLevel ASC, RegistrationDate ASC, OrderId ASC`.

---

## 4. Historial Secuencial de Prompts

A continuación se detalla cada prompt utilizado para guiar al agente OpenCode a través del ciclo de vida del desarrollo:

### Paso 1: Configuración de Convenciones (Spec / Plan / Tasks)
> **Objetivo:** Definir identificadores técnicos en inglés y documentación/UI en español, aprobando la creación de los artefactos SDD iniciales.

```text
Aprobado. Procede a generar spec.md, plan.md y tasks.md dentro de specs/001-pedidos-priority-mvp/.


Ejecuta la Fase 0 y Fase 1 de @specs/001-pedidos-priority-mvp/tasks.md con estos parámetros exactos:

- ConnectionString a colocar en appsettings.json:
  "Server=localhost;Database=PedidosDb;Trusted_Connection=True;TrustServerCertificate=True;"

- Nombres oficiales de Stored Procedures:
  * sp_InsertOrder
  * sp_GetPackingQueue

Instrucciones:
1. Crea la solución y el proyecto MVC con la estructura de carpetas definida.
2. Instala Microsoft.Data.SqlClient y configura el ConnectionString en appsettings.json.
3. Genera en SqlScripts/ los archivos .sql completos para la creación de la base de datos, tablas y ambos procedimientos almacenados.
4. Detente al terminar la Fase 1 para que yo ejecute los scripts en SQL Server. No avances a la Fase 2 todavía.


La base de datos PedidosDb y los Stored Procedures ya están creados y probados con éxito en SQL Server.

Actualización del modelo de datos:
- Se normalizó la tabla Orders incorporando relación con Customers (CustomerId, FirstName, LastName, Phone, DeliveryAddress) y campos de importe (Subtotal, ShippingCost, Total, PriorityLevel, RegistrationDate).
- Los SPs oficiales implementados y validados son:
  * dbo.sp_InsertOrder (@CustomerId, @Subtotal, @ShippingCost, @OrderId OUTPUT)
  * dbo.sp_GetPackingQueue (retorna OrderId, CustomerFullName, CustomerPhone, DeliveryAddress, Subtotal, ShippingCost, Total, PriorityLevel, PriorityName, RegistrationDate)

Credenciales y configuración:
- ConnectionString para appsettings.json:
  "Server=localhost;Database=PedidosDb;User Id=sa;Password=TU_PASSWORD_AQUI;TrustServerCertificate=True;"

Instrucciones:
1. Actualiza @specs/001-pedidos-priority-mvp/spec.md con estos contratos actualizados.
2. Ejecuta la Fase 0 y Fase 2 de tasks.md: crea el proyecto ASP.NET Core MVC, agrega el paquete Microsoft.Data.SqlClient, configura la cadena de conexión en appsettings.json, y crea las entidades, interfaces y el repositorio OrderRepository invocando los Stored Procedures mediante SqlCommand y SqlParameter.
3. Ejecuta dotnet build para verificar que compile limpiamente.


Excelente. Procede a ejecutar la Fase 3 y la Fase 4 de @specs/001-pedidos-priority-mvp/tasks.md:

1. Fase 3 (Servicio):
   - Implementa `Services/IOrderService.cs` y `Services/OrderService.cs`.
   - Incluye la validación de dominio (Subtotal > 0, ShippingCost >= 0, CustomerId válido) y la orquestación hacia `IOrderRepository`.
   - Registra `IOrderService` en el contenedor de dependencias de `Program.cs`.

2. Fase 4 (Pruebas Unitarias con xUnit):
   - Crea el proyecto de pruebas `PedidosPriority.Tests` y agrégalo a la solución.
   - Implementa un repositorio mock/falso (`FakeOrderRepository`) para probar el servicio de forma aislada sin requerir conexión a SQL Server.
   - Crea pruebas unitarias que verifiquen:
     * Regla de prioridad: ShippingCost > 0 produce prioridad ALTA (1).
     * Regla de prioridad: ShippingCost = 0 produce prioridad BAJA (2).
     * Validación: ShippingCost < 0 arroja excepción de validación.
     * Ordenamiento de cola: pedidos de prioridad 1 van antes que prioridad 2.

3. Verificación:
   - Ejecuta dotnet test y asegúrate de que todas las pruebas pasen en verde.
   - Marca las tareas completadas en tasks.md.

Excelente trabajo. Procede a ejecutar la Fase 5 y la Fase 6 de @specs/001-pedidos-priority-mvp/tasks.md:

1. Fase 5 (Presentación):
   - Implementa `Controllers/EntregapedidosController.cs` con las acciones:
     * `GET Index`: invoca `IOrderService.GetPackingQueue()` y pasa el modelo a la vista.
     * `POST Create`: recibe el formulario, invoca `IOrderService.CreateOrder(...)` y aplica el patrón PRG (RedirectToAction a Index).
   - Crea la vista Razor en `Views/Entregapedidos/Index.cshtml` con:
     * Formulario superior para registrar pedidos: selección de cliente, Subtotal y Costo de Envío.
     * Tabla con la cola de empaquetamiento: Turno, Cliente, Teléfono, Dirección, Subtotal, Costo de Envío, Total, y Badges en español ("ALTA" en rojo / "BAJA" en verde) junto al tipo de servicio ("Envío Express" / "Envío Gratis").
   - Configura la ruta predeterminada en `Program.cs` para que navegue directamente a `/Entregapedidos`.
   - Usa estilos CSS limpios y JS vanilla (sin librerías externas pesadas).

2. Fase 6 (Verificación y Auditoría):
   - Ejecuta dotnet build y dotnet test para asegurar compilación limpia y pruebas en verde.
   - Realiza la auditoría de cero SQL inline en archivos .cs.
   - Marca todas las tareas pendientes como completadas en tasks.md.

Ajusta la interfaz para separar el registro de la cola de empaquetamiento:

1. Pantalla de Cola de Empaquetamiento (/entregapedidos):
   - Debe contener ÚNICAMENTE la tabla "PEDIDOS EN COLA" (Turno, Cliente, Teléfono, Dirección, Subtotal, Costo de Envío, Total, Prioridad con badges ALTA/BAJA y tipo de Envío).
   - Quita el formulario de registro de esta vista.
   - Si no hay pedidos, muestra un mensaje informativo ("No hay pedidos pendientes en la cola").

2. Pantalla Principal de Registro (/ o /Pedidos):
   - Crea una vista para registrar pedidos con el formulario: Cliente (ID o Selector), Subtotal y Costo de Envío.
   - Al hacer clic en "Agregar a la cola", guarda el pedido mediante IOrderService y redirige automáticamente a `/entregapedidos` para que se observe cómo el pedido entra en su posición correspondiente.

3. Menú de Navegación (_Layout.cshtml):
   - Enlace "Registrar Pedido" apuntando al formulario (`/`).
   - Enlace "Entrega de Pedidos" apuntando a la cola (`/entregapedidos`).

Ejecuta dotnet build para comprobar que compile sin errores.

La interfaz actual se ve demasiado plana y genérica. Sin agregar ningún framework externo (manteniendo CSS vanilla puro en wwwroot/css/site.css), mejora el diseño visual para que parezca una aplicación web moderna:

1. Estética General:
   - Fondo general gris muy suave (#f8fafc) y tipografía de sistema limpia (Inter, Segoe UI, sans-serif).
   - Barra de navegación superior moderna (blanca, sombra sutil, logotipo destacado en negrita y enlaces con hover elegante).

2. Formulario de Registro (/):
   - Tarjeta centrada con sombra suave (box-shadow), bordes redondeados (border-radius: 12px) y padding generoso.
   - Inputs estilizados con bordes sutiles, enfoque visible (focus con borde azul y halo suave) y labels en negrita gris oscuro.
   - Botón principal llamativo con gradiente o color azul sólido (#2563eb), efecto hover y cursor pointer.

3. Tabla de Cola de Empaquetamiento (/entregapedidos):
   - Tabla limpia dentro de una tarjeta blanca contenedora.
   - Encabezados con fondo gris suave, texto en mayúsculas pequeñas (font-size: 0.75rem, letter-spacing).
   - Filas con padding cómodo y efecto hover al pasar el ratón.
   - Badges de prioridad estilizados:
     * ALTA: fondo rojizo suave (#fee2e2), texto rojo fuerte (#b91c1c), bordes redondeados y texto en negrita.
     * BAJA: fondo verde suave (#dcfce7), texto verde fuerte (#15803d), bordes redondeados.
   - Resalta visualmente el "Turno" o ID y la columna "Total".

Mantén todo el código 100% en `wwwroot/css/site.css` y las vistas Razor existentes, sin instalar paquetes ni librerías externas.
