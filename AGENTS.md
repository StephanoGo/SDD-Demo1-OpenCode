# AGENTS.MD: Contexto Operativo y Directrices de la Máquina (OpenCode)

## 1. Identidad y Propósito
Este archivo define el entorno operativo, permisos y restricciones de ejecución para el agente de IA dentro del repositorio. Tu objetivo principal es implementar de forma determinista las especificaciones técnicas definidas bajo la metodología Spec-Driven Development (SDD), minimizando el consumo de tokens y evitando la sobreingeniería.

---

## 2. Jerarquía de Verdad (Orden de Precedencia)
1. `docs/constitution.md` (Leyes inmutables del proyecto).
2. `specs/001-pedidos-priority-mvp/spec.md` (Requisitos de negocio, contratos y procedimientos).
3. `specs/001-pedidos-priority-mvp/plan.md` y `tasks.md` (Planificación y desglose de ejecución).
4. `AGENTS.md` (Este archivo: límites operativos y convenciones de la máquina).

---

## 3. Comandos Autorizados

El agente tiene autorización estricta para ejecutar únicamente los siguientes comandos en la terminal integrada:

* **Compilación y Construcción:**
  - `dotnet build`
* **Ejecución de Pruebas:**
  - `dotnet test`
  - `dotnet test --filter <NombreDeLaPrueba>`
* **Gestión de Paquetes NuGet (Estrictamente el cliente oficial de SQL Server):**
  - `dotnet add package Microsoft.Data.SqlClient`
* **Inspección de Archivos y Directorios:**
  - `ls`, `dir` (solo lectura)

### Comandos Prohibidos:
- Queda prohibido el uso de comandos destructivos (`rm -rf`, `del /f`, `git reset --hard`).
- Queda prohibida la instalación de ORMs pesados (Entity Framework Core) o frameworks frontend externos (npm install react, vue, angular, tailwind).
- Queda prohibido levantar servicios o servidores de ejecución infinita que bloqueen la terminal (`dotnet run` o `dotnet watch`), a menos que sea solicitado explícitamente para validación manual.

---

## 4. Convenciones de Nombres e Idiomas

* **Idioma del Código:** **Inglés**
  - Todo identificador en el código fuente debe escribirse en inglés:
    - Nombres de clases, interfaces, métodos y propiedades (`Order`, `IOrderRepository`, `OrderService`, `ShippingCost`, `PriorityLevel`).
    - Nombres de tablas y columnas en SQL Server (`Orders`, `ShippingCost`, `PriorityLevel`, `RegistrationDate`).
    - Nombres de archivos `.cs`, `.js` y scripts `.sql`.
* **Idioma de Interacción y Documentación:** **Español**
  - Respuestas, resúmenes de tareas y mensajes de confirmación al usuario en la terminal.
  - Documentación técnica (`spec.md`, `constitution.md`, `tasks.md`).
  - Textos visibles de la interfaz de usuario en las vistas MVC (labels de formularios, encabezados de tablas, badges de prioridad: "ALTA", "BAJA", "Envío Gratis").

---

## 5. Restricciones Técnicas y Límites de Modificación

### Lo que el agente NO DEBE TOCAR:
1. **Lógica SQL Inline:** No escribas consultas SQL embebidas (`"SELECT * FROM..."`) dentro de archivos `.cs`. Toda operación de base de datos debe residir exclusivamente en archivos de Stored Procedures (`SqlScripts/*.sql`) y ejecutarse vía `SqlCommand` con `CommandType.StoredProcedure` y parámetros tipados (`SqlParameter`).
2. **Archivos de Configuración de la Solución:** No modifiques `PedidosPriority.sln`, `Program.cs` (salvo para registrar dependencias de servicios/repositorio) ni archivos `.csproj` sin orden expresa en `tasks.md`.
3. **Estructura de Capas:** No colapses capas ni crees proyectos adicionales. Mantén la separación en 3 capas lógicas (`Presentation`, `Application/Services`, `Data/Repositories`).

---

## 6. Protocolo de Respuesta
- Al completar una tarea de `tasks.md`, confirma de manera concisa:
  1. Los archivos creados o actualizados.
  2. El comando de verificación ejecutado (`dotnet build` o `dotnet test`).
  3. El estado de la tarea en la lista de verificación (checklist).