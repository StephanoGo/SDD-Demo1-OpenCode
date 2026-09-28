# Plan 002 — Estrategia Técnica del Rediseño Visual

## 1. Objetivo
Rediseñar la presentación fiel al prototipo Figma **sin tocar el backend**: solo vistas, layout, CSS y JS vanilla.

## 2. Alcance
**Archivos a modificar:**
- `PedidosPriority/Views/Shared/_Layout.cshtml` — header con isotipo + píldora de 3 pestañas, footer global.
- `PedidosPriority/Views/Pedidos/Index.cshtml` — formulario (tarjeta, íconos, presets, total estimado).
- `PedidosPriority/Views/Entregapedidos/Index.cshtml` — subtítulo, toolbar, tabla refinada, footer de métricas.
- `PedidosPriority/wwwroot/css/site.css` — tokens, componentes, reescritura completa del diseño.
- `PedidosPriority/wwwroot/js/site.js` — presets + total reactivo (reemplaza el listener actual de `.money-input`).

**No se tocan:** controllers, servicios, repositorios, `Program.cs`, modelos (`CreateOrderRequest`, `Order`), SPs, `.slnx`, `.csproj`.

## 3. Estrategia Técnica
- CSS vanilla puro: variables CSS (`:root`), Flexbox/Grid, desktop-first.
- Iconos inline SVG embebidos (usuario, ubicación, camión, check, isotipo del logo). Sin librerías.
- Marcado `data-*` en presets (`data-shipping-preset` con valor `0`/`15.00`) para el JS.
- Un único handler DOMContentLoaded en `site.js` que unifica: formateo `.money-input`, presets y cálculo de total.

## 4. Detalle por Componente
| Vista | Componentes |
|-------|-------------|
| `_Layout` | navbar sticky (isotipo + píldora tabs, 3.ª inactiva), `footer` con © 2026 SaaS |
| `Pedidos/Index` | `card` centrada: título, subtítulo, grupo Cliente (ícono+ayuda), Subtotal "S/", presets + nota ALTA, caja `#TotalEstimado`, botón ancho |
| `Entregapedidos/Index` | título+subtítulo, toolbar (buscador/filtros), tabla con columnas §6 del spec, Turno `.ToString("00")`, columna ACCIÓN, footer métricas |

## 5. JS Vanilla (`site.js`)
- Al cargar: enlazar eventos a `.shipping-preset` (click) y a `#Subtotal`/`#ShippingCost` (input).
- `recalculateTotal()`: `total = subtotal + shippingCost`; renderiza en `#TotalEstimado` con formato "S/ ".
- Presets: `input.value = preset`; luego `recalculateTotal()`.
- Mantener el redondeo `toFixed(2)` al hacer blur de los inputs monetarios.

## 6. Verificación
- `dotnet build` → 0 errores, 0 advertencias.
- `dotnet test` → 8/8 en verde.
- Auditoría visual manual (paleta, píldora, presets, tabla, footer).
- Auditoría de dependencias: sin enlaces a `cdn`, `npm`, Bootstrap, jQuery, fuentes externas (la fuente Inter es *fallback* del sistema, sin descarga).