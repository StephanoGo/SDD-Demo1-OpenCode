# CONSTITUCIÓN DEL PROYECTO: Sistema de Priorización y Empaquetamiento de Pedidos

## 1. Simplicidad del Stack
- Mantener la base tecnológica lo más limpia, ligera y libre de fricción posible.
- Stack tecnológico exclusivo: **ASP.NET Core MVC** (.NET 8/9), **C#**, **Vanilla JavaScript (ES6+)**, **HTML5/CSS3** y **Microsoft SQL Server**.
- Cero dependencias innecesarias: se prohíbe la instalación de ORMs pesados, librerías frontend de terceros (como Bootstrap pesado, jQuery, React o Tailwind) o paquetes NuGet redundantes.
- El acceso a datos se gestiona únicamente mediante el cliente oficial nativo: `Microsoft.Data.SqlClient`.

---

## 2. Relación Spec-Código
- **La especificación (`spec.md`) manda de forma absoluta:** ningún desarrollador ni agente de IA escribirá una sola línea de código, endpoint, propiedad o vista que no esté documentada previamente.
- Si surge ambigüedad durante la implementación, el agente debe detenerse y pedir clarificación técnica antes de asumir comportamientos por conjetura.
- Todo cambio en el comportamiento del sistema requiere una actualización previa en `spec.md` y su desglose correspondiente en `tasks.md`.

---

## 3. Separación de Capas
- Arquitectura estricta en **3 capas lógicas**:
  1. **Capa de Presentación (`Controllers/`, `Views/`, `wwwroot/`):** Encargada exclusivamente del manejo de peticiones HTTP, validación de modelos (`ModelState`), renderizado de vistas Razor y respuestas JSON. Prohibido incluir lógica de cálculo de negocio o sentencias de base de datos en los controladores.
  2. **Capa de Lógica / Servicio (`Services/`):** Implementa las reglas de negocio puras (cálculo de prioridades y niveles) y orquesta las llamadas al repositorio mediante contratos de interfaz (`IPedidosService`).
  3. **Capa de Acceso a Datos / Persistencia (`Data/`, `Models/`):** Implementa contratos (`IPedidosRepository`) y encapsula el acceso a SQL Server. La lógica de negocio jamás interactúa de forma directa con la conexión o los comandos de base de datos.

---

## 4. Persistencia de Datos y Seguridad en Base de Datos
- Motor de persistencia: **Microsoft SQL Server**.
- **Tolerancia cero a SQL Injection:** Queda terminantemente prohibido el uso de SQL inline, concatenación de cadenas (`string concatenation`) o interpolación de texto SQL en el código C#.
- **Uso obligatorio de Stored Procedures:** Toda operación de inserción, actualización o lectura debe realizarse invocando procedimientos almacenados mediante `SqlCommand` con `CommandType.StoredProcedure` y parámetros fuertemente tipados (`SqlParameter`).
- Todos los scripts de esquema DDL (tablas) y procedimientos almacenados deben residir versionados dentro del repositorio en la carpeta `SqlScripts/`.

---

## 5. Política de Tests
- Todo componente central de negocio debe ser verificable y reproducible de forma automatizada mediante proyectos de pruebas (`xUnit` o `NUnit`).
- Las pruebas unitarias son obligatorias para:
  1. La regla de cálculo de prioridad (`CostoEnvio > 0` => ALTA, `CostoEnvio == 0` => BAJA).
  2. El criterio determinista de ordenamiento en la cola (`PrioridadNivel ASC`, `FechaRegistro ASC`).
- Ninguna tarea se considerará finalizada en `tasks.md` si el código no compila limpiamente y pasa las pruebas de validación.

---

## 6. Idioma y Convenciones
- **Código y Nombres Técnicos:** Nombres de clases, métodos, interfaces, entidades, parámetros y base de datos se escribirán en **Español** con convención PascalCase (ejemplo: `Pedido`, `CostoEnvio`, `IPedidosService`, `sp_InsertarPedido`).
- **Comentarios y Documentación:** Todos los comentarios técnicos, especificaciones y archivos markdown se mantendrán en **Español neutro**.
- **Interfaz de Usuario (UI):** Todos los mensajes, etiquetas de formulario, botones, encabezados de tabla y badges de estado se mostrarán en **Español** (ejemplo: `Turno`, `Cliente`, `Envío Express`, `Prioridad ALTA`).