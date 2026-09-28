# Tasks 002 — Checklist de Implementación del Rediseño

> Criterio de finalización: cada tarea compila limpiamente (`dotnet build`) y, al cierre, `dotnet test` 8/8.

## T1 — Layout & Paleta

- [x] T1.1 Definir tokens CSS en `site.css` (paleta crema, acento `#2563eb`/`#3b82f6`, `--radius-card: 16px`, `--radius-pill`, sombras difusas).
- [x] T1.2 `_Layout.cshtml`: navbar con isotipo + píldora de 3 pestañas ("Registrar Pedido", "Pedidos en Cola", "Entrega de Pedidos" inactiva).
- [x] T1.3 Footer global: "© 2026 - PedidosPriority SaaS. Todos los derechos reservados."
- [x] T1.4 Verificación: `dotnet build` sin errores.

## T2 — Formulario Registro (`/`)

- [x] T2.1 Tarjeta centrada con encabezado "Registrar Pedido" + subtítulo descriptivo.
- [x] T2.2 Input Cliente (ID) con ícono de usuario y texto de ayuda.
- [x] T2.3 Input Subtotal con prefijo "S/".
- [x] T2.4 Presets de envío "Gratis (S/ 0)" y "Express (S/ 15.00)" + nota sobre prioridad ALTA.
- [x] T2.5 Caja inferior "TOTAL ESTIMADO" (placeholder para JS) y botón ancho "+ Agregar a la cola".
- [x] T2.6 Verificación: `dotnet build` sin errores.

## T3 — Tabla Cola & Badges (`/entregapedidos`)

- [x] T3.1 Título "Pedidos en Cola" + subtítulo operativo.
- [x] T3.2 Barra de utilidades: buscador y botón "Filtros" (placeholders).
- [x] T3.3 Turno en badge con relleno 2 dígitos (`#01`, `#02`).
- [x] T3.4 Columna Cliente (negrita + ID), dirección con ícono, envío "GRATIS"/"S/ monto", Total destacado.
- [x] T3.5 Badges prioridad pill con punto: ALTA (`#fee2e2`/`#b91c1c`), BAJA gris neutro (`#f1f5f9`/`#475569`).
- [x] T3.6 Tipo de Entrega (camión Express / check Gratis) y columna "ACCIÓN" con botón "Despachar >".
- [x] T3.7 Footer de tabla: "Mostrando X pedidos activos en preparación | Tiempo estimado medio: 18 mins".
- [x] T3.8 Verificación: `dotnet build` sin errores.

## T4 — Vanilla JS reactivo (`site.js`)

- [x] T4.1 Presets autocompletan `#ShippingCost` (0 / 15.00).
- [x] T4.2 Cálculo en tiempo real de "TOTAL ESTIMADO" (`Subtotal + ShippingCost`).
- [x] T4.3 Formato "S/ N,N0" y redondeo a 2 decimales; conservar formateo `.money-input`.
- [x] T4.4 Verificación: `dotnet build` sin errores.

## T5 — Verificación y Cierre

- [x] T5.1 `dotnet build` → 0 errores, 0 advertencias.
- [x] T5.2 `dotnet test` → 8/8 en verde.
- [x] T5.3 Auditoría visual manual contra el prototipo (paleta, píldora, presets, tabla, footers).
- [x] T5.4 Auditoría de dependencias externas → cero (sin cdn/npm/Bootstrap/jQuery/fuentes descargadas).
- [x] T5.5 Marcar todos los checklists como completados.

## Contratos Imprescindibles (no negociables)

1. Cero librerías/frameworks externos en HTML/CSS/JS.
2. Sin cambios en backend, modelos, SPs, `.slnx` ni `.csproj`.
3. UI en español; identificadores técnicos en inglés (decisión D1 del spec 001).
4. Presets fijos: Gratis = `0`, Express = `15.00`.
5. Turno con relleno de ceros a 2 dígitos (`#01`, `#02`).