# Spec 002 — Rediseño Visual de UI (fiel al prototipo Figma)

## 1. Visión General

Rediseño visual de la capa de presentación del caso de uso 001, fiel al prototipo de Figma.
**No altera** contratos de datos, Stored Procedures, reglas de prioridad, ni modelos (`CreateOrderRequest`/`Order`).
El backend (controllers/servicios/repositorios) permanece intacto; los cambios son exclusivamente de presentación
(`Views/`, `_Layout.cshtml`, `wwwroot/css/site.css`, `wwwroot/js/site.js`).

## 2. Decisiones Registradas

| # | Decisión | Justificación |
|---|----------|---------------|
| D1 | Texto visible de la UI en **español**. | AGENTS.md §4 y constitution §6. |
| D2 | **CSS vanilla puro**: cero Bootstrap/Tailwind/librerías de iconos. Iconos inline SVG embebidos en las vistas. | Restricción del proyecto; diseño fiel a Figma. |
| D3 | Moneda PEN con prefijo **"S/"** únicamente en presentación. El formulario sigue enviando `decimal` (`Subtotal`, `ShippingCost`). | No cambia el contrato de datos del spec 001. |
| D4 | Presets de envío **fijos**: "Gratis (S/ 0)" y "Express (S/ 15.00)". Son atajos que **llenan el input numérico** `ShippingCost`; el valor persistido es el del input (0 o 15.00). | Confirmado por el usuario. |
| D5 | Columna "ACCIÓN" con botón "Despachar >", buscador y "Filtros" son **placeholders** preparados para el siguiente feature (no invocan lógica). | Fuera de alcance; solo disposición visual. |
| D6 | Turno en la tabla con relleno de ceros a 2 dígitos: `#01`, `#02`, … | Confirmado por el usuario. |

## 3. Paleta y Tipografía

| Token CSS | Valor | Uso |
|-----------|-------|-----|
| `--bg` | `#faf8f3` | Fondo general crema/marfil suave |
| `--surface` | `#ffffff` | Tarjetas, tabla |
| `--accent` | `#2563eb` | Botones de acción, resaltados |
| `--accent-light` | `#3b82f6` | Hover/variantes |
| `--danger-bg` | `#fee2e2` | Pill ALTA (fondo) |
| `--danger-text` | `#b91c1c` | Pill ALTA (texto/punto) |
| `--neutral-bg` | `#f1f5f9` | Pill BAJA (fondo) |
| `--neutral-text` | `#475569` | Pill BAJA (texto/punto) |
| `--radius-card` | `16px` | Tarjetas (registro, tabla, header) |
| `--radius-pill` | `999px` | Píldoras y badges |

- Tipografía: `Inter, "Segoe UI", system-ui, sans-serif`.
- Sombras difusas sutiles (p.ej. `0 8px 24px rgba(15, 23, 42, 0.08)`).

## 4. Header / Navbar

- Logotipo "PedidosPriority" con **isotipo** (marca decorativa CSS/SVG).
- Píldora de navegación con **tres pestañas**:
  1. "Registrar Pedido" → `/` (activa).
  2. "Pedidos en Cola" → `/entregapedidos` (activa).
  3. "Entrega de Pedidos" → **inactiva/placeholder** (estado futuro, sin enlace funcional).

## 5. Vista Registro de Pedido (`/`)

- Tarjeta centrada con encabezado "Registrar Pedido" + subtítulo descriptivo.
- **Input Cliente (ID)**: ícono de usuario + texto de ayuda (p.ej. "Ingresa el ID del cliente").
- **Input Subtotal**: con prefijo "S/" visible.
- **Costo de envío**: botones rápidos preset:
  - "Gratis (S/ 0)" → rellena `ShippingCost = 0`.
  - "Express (S/ 15.00)" → rellena `ShippingCost = 15.00`.
  - Nota explicativa: "El envío con costo asigna prioridad ALTA en la cola".
- **Caja resumen inferior**: "TOTAL ESTIMADO" actualizado en tiempo real (vanilla JS) y botón ancho "+ Agregar a la cola".

## 6. Vista Pedidos en Cola (`/entregapedidos`)

- Título "Pedidos en Cola" + subtítulo operativo.
- Barra de utilidades superior: buscador (cliente/dirección) y botón "Filtros" — **placeholders estéticos** (sin funcionalidad).
- Tabla: Turno en badge `#01`, `#02`, … (2 dígitos); Cliente en negrita + ID; teléfono; dirección con ícono de ubicación; Subtotal; Envío ("GRATIS" o monto con prefijo "S/"); Total destacado; badges de prioridad pill con punto (ALTA rojo suave / BAJA gris neutro); Tipo de Entrega (Express con ícono camión / Gratis con check).
- Columna "ACCIÓN": botón azul "Despachar >" (preparado para el siguiente feature, sin lógica).
- Footer de la tabla: "Mostrando X pedidos activos en preparación | Tiempo estimado medio: 18 mins" (X = conteo real del modelo; "18 mins" placeholder fijo).
- Footer global: "© 2026 - PedidosPriority SaaS. Todos los derechos reservados."

## 7. Contrato JS (vanilla, `wwwroot/js/site.js`)

- Selectores: `#Subtotal`, `#ShippingCost`, `.shipping-preset`, `#TotalEstimado`.
- Presets: al hacer clic, establecen `ShippingCost` a `0` o `15.00` y refrescan el total.
- Reacción: recalcular `Total = Subtotal + ShippingCost` en `input` del subtotal, en `input` del envío y al pulsar presets; redondeo a 2 decimales y formato "S/ N,N0".
- Se conserva el formateo `toFixed(2)` de los inputs monetarios existente.
- Cero `innerHTML` con datos no sanitizados; sin librerías.

## 8. Criterios de Aceptación

1. El registro (`/`) muestra tarjeta centrada con encabezado + subtítulo, íconos, ayuda, prefijo "S/" y caja TOTAL ESTIMADO.
2. Los presets llenan el input: "Gratis" → `0`, "Express" → `15.00`, y el total estimado se actualiza al instante.
3. La cola (`/entregapedidos`) muestra título + subtítulo, toolbar placeholder, tabla con TODAS las columnas del §6, Turno `#01`/`#02`, pills con punto y footer de métricas.
4. El navbar muestra la píldora de 3 pestañas; "Entrega de Pedidos" está inactiva.
5. **Cero** frameworks/librerías externas en `html`, `css` y `js`.
6. `dotnet build` → 0 errores; `dotnet test` → 8/8 en verde (sin regresiones).