document.addEventListener('DOMContentLoaded', function () {
    var shippingInput = document.getElementById('ShippingCost');
    var totalElement = document.getElementById('TotalEstimado');
    var presets = document.querySelectorAll('.shipping-preset');
    var moneyInputs = document.querySelectorAll('.money-input');

    function formatMoney(value) {
        return value.toLocaleString('es-PE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function recalculateTotal() {
        if (!totalElement) {
            return;
        }

        var shipping = parseFloat(shippingInput ? shippingInput.value : 0) || 0;

        totalElement.textContent = 'S/ ' + formatMoney(subtotalGeneral() + shipping);
    }

    presets.forEach(function (button) {
        button.addEventListener('click', function () {
            if (!shippingInput) {
                return;
            }

            shippingInput.value = parseFloat(button.dataset.shippingPreset || '0').toFixed(2);

            presets.forEach(function (other) {
                other.classList.toggle('selected', other === button);
            });

            recalculateTotal();
        });
    });

    moneyInputs.forEach(function (input) {
        input.addEventListener('blur', function () {
            var value = parseFloat(input.value);

            if (!isNaN(value)) {
                input.value = value.toFixed(2);
            }
        });
    });

    if (shippingInput) {
        shippingInput.addEventListener('input', recalculateTotal);
    }

    var buscarCliente = document.getElementById('buscar-cliente');
    var clienteSugerencias = document.getElementById('cliente-sugerencias');
    var clienteSeleccionado = document.getElementById('cliente-seleccionado');
    var customerIdHidden = document.getElementById('customer-id-hidden');
    var categoriaSelect = document.getElementById('categoria-select');
    var productoSelect = document.getElementById('producto-select');
    var cantidadInput = document.getElementById('cantidad-input');
    var btnAgregarItem = document.getElementById('btn-agregar-item');
    var itemsBody = document.getElementById('items-body');
    var subtotalGeneralEl = document.getElementById('subtotal-general');
    var ordenForm = document.getElementById('form-orden');
    var items = [];
    var selectedProduct = null;

    function subtotalGeneral() {
        return items.reduce(function (acc, item) {
            return acc + (item.quantity * item.unitPrice);
        }, 0);
    }

    function renderItems() {
        if (!itemsBody) {
            return;
        }

        itemsBody.innerHTML = '';

        items.forEach(function (item, index) {
            var row = document.createElement('tr');

            row.appendChild(buildCell('td', null, item.productName));
            row.appendChild(buildCell('td', null, String(item.quantity)));
            row.appendChild(buildCell('td', 'text-right', 'S/ ' + formatMoney(item.unitPrice)));
            row.appendChild(buildCell('td', 'text-right', 'S/ ' + formatMoney(item.quantity * item.unitPrice)));

            var tdRemove = document.createElement('td');
            tdRemove.className = 'text-right';

            var removeButton = document.createElement('button');
            removeButton.type = 'button';
            removeButton.className = 'btn-remove-item';
            removeButton.textContent = 'Quitar';
            removeButton.setAttribute('aria-label', 'Quitar ítem');
            removeButton.addEventListener('click', function () {
                items.splice(index, 1);
                renderItems();
                recalculateTotal();
            });

            tdRemove.appendChild(removeButton);
            row.appendChild(tdRemove);

            itemsBody.appendChild(row);
        });

        if (subtotalGeneralEl) {
            subtotalGeneralEl.textContent = 'S/ ' + formatMoney(subtotalGeneral());
        }
    }

    if (buscarCliente && clienteSugerencias && customerIdHidden) {
        fetch(window.catalogUrls.clientes)
            .then(function (response) { return response.json(); })
            .then(function (data) {
                window.clientesCache = data;
                if (buscarCliente.value.trim() !== '') {
                    filterCustomers(buscarCliente.value);
                }
            });

        buscarCliente.addEventListener('input', function () {
            if (window.clientesCache) {
                filterCustomers(buscarCliente.value);
            }
        });

        buscarCliente.addEventListener('blur', function () {
            setTimeout(function () {
                clienteSugerencias.hidden = true;
            }, 150);
        });

        buscarCliente.addEventListener('focus', function () {
            if (window.clientesCache && buscarCliente.value.trim() !== '' && clienteSugerencias.hidden) {
                filterCustomers(buscarCliente.value);
            }
        });
    }

    function buildCell(tag, className, text) {
        var cell = document.createElement(tag);

        if (className) {
            cell.className = className;
        }

        cell.textContent = text;

        return cell;
    }

    function filterCustomers(query) {
        var q = (query || '').trim().toLowerCase();
        var matches;

        clienteSugerencias.innerHTML = '';

        if (q === '') {
            clienteSugerencias.hidden = true;
            return;
        }

        matches = (window.clientesCache || []).filter(function (customer) {
            var haystack = String(customer.customerId) + ' ' +
                (customer.fullName || '') + ' ' +
                (customer.firstName || '') + ' ' +
                (customer.lastName || '');
            return haystack.toLowerCase().indexOf(q) !== -1;
        });

        matches.slice(0, 8).forEach(function (customer) {
            var option = document.createElement('button');
            option.type = 'button';
            option.className = 'cust-suggest-item';

            var name = document.createElement('strong');
            name.textContent = customer.fullName || (customer.firstName + ' ' + customer.lastName);

            var meta = document.createElement('span');
            meta.textContent = 'ID ' + customer.customerId + ' · ' + (customer.phone || 'Sin teléfono');

            option.appendChild(name);
            option.appendChild(meta);

            option.addEventListener('click', function () {
                selectCustomer(customer);
            });

            clienteSugerencias.appendChild(option);
        });

        clienteSugerencias.hidden = matches.length === 0;
    }

    function selectCustomer(customer) {
        customerIdHidden.value = customer.customerId;

        if (clienteSeleccionado) {
            clienteSeleccionado.textContent = '✓ ' + (customer.fullName || '') + ' (ID ' + customer.customerId + ')';
            clienteSeleccionado.hidden = false;
        }

        buscarCliente.value = customer.fullName || '';
        clienteSugerencias.innerHTML = '';
        clienteSugerencias.hidden = true;
    }

    if (categoriaSelect && productoSelect && btnAgregarItem) {
        fetch(window.catalogUrls.categorias)
            .then(function (response) { return response.json(); })
            .then(function (data) {
                categoriaSelect.innerHTML = '';

                var placeholder = document.createElement('option');
                placeholder.value = '';
                placeholder.textContent = 'Selecciona una categoría...';
                categoriaSelect.appendChild(placeholder);

                data.forEach(function (category) {
                    var option = document.createElement('option');
                    option.value = category.categoryId;
                    option.textContent = category.name;
                    categoriaSelect.appendChild(option);
                });
            });

        categoriaSelect.addEventListener('change', loadProducts);

        productoSelect.addEventListener('change', function () {
            var option = productoSelect.selectedOptions[0];

            selectedProduct = option && option.value
                ? { id: Number(option.value), name: option.dataset.name, price: Number(option.dataset.price), stock: Number(option.dataset.stock) }
                : null;
        });

        btnAgregarItem.addEventListener('click', addItem);
    }

    function loadProducts() {
        var categoryId = categoriaSelect.value;

        productoSelect.innerHTML = '';
        selectedProduct = null;
        productoSelect.disabled = !categoryId;

        if (!categoryId) {
            var placeholder = document.createElement('option');
            placeholder.value = '';
            placeholder.textContent = 'Primero elige una categoría';
            productoSelect.appendChild(placeholder);
            return;
        }

        fetch(window.catalogUrls.productos + '?categoriaId=' + categoryId)
            .then(function (response) { return response.json(); })
            .then(function (data) {
                productoSelect.innerHTML = '';

                var hint = document.createElement('option');
                hint.value = '';
                hint.textContent = data.length ? 'Selecciona un producto...' : 'Sin productos disponibles (stock)';
                productoSelect.appendChild(hint);

                data.forEach(function (product) {
                    var option = document.createElement('option');
                    option.value = product.productId;
                    option.dataset.name = product.productName;
                    option.dataset.price = product.unitPrice;
                    option.dataset.stock = product.stock;
                    option.textContent = product.productName + ' — S/ ' + formatMoney(product.unitPrice) + ' (stock ' + product.stock + ')';
                    productoSelect.appendChild(option);
                });
            });
    }

    function addItem() {
        if (!selectedProduct) {
            alert('Selecciona un producto del catálogo.');
            return;
        }

        var quantity = parseInt(cantidadInput.value, 10) || 0;

        if (quantity < 1) {
            alert('La cantidad debe ser mayor a cero.');
            return;
        }

        if (quantity > selectedProduct.stock) {
            alert('Stock insuficiente (disponible: ' + selectedProduct.stock + ').');
            return;
        }

        items.push({
            productId: selectedProduct.id,
            productName: selectedProduct.name,
            quantity: quantity,
            unitPrice: selectedProduct.price
        });

        renderItems();
        recalculateTotal();

        cantidadInput.value = '1';
        productoSelect.value = '';
        selectedProduct = null;
    }

    function syncHiddenItems() {
        if (!ordenForm) {
            return;
        }

        ordenForm.querySelectorAll('.item-hidden').forEach(function (input) {
            input.remove();
        });

        items.forEach(function (item, index) {
            appendHidden(ordenForm, 'Items[' + index + '].ProductId', item.productId);
            appendHidden(ordenForm, 'Items[' + index + '].Quantity', item.quantity);
            appendHidden(ordenForm, 'Items[' + index + '].UnitPrice', item.unitPrice);
        });
    }

    function appendHidden(form, name, value) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value;
        input.className = 'item-hidden';
        form.appendChild(input);
    }

    if (ordenForm) {
        ordenForm.addEventListener('submit', function (event) {
            if (items.length === 0) {
                event.preventDefault();
                alert('Agrega al menos un producto antes de registrar el pedido.');
                return;
            }

            syncHiddenItems();
        });
    }

    var detalleInput = document.getElementById('detalle-id');
    var detalleBtn = document.getElementById('btn-ver-detalle');

    function openOrderDetail() {
        var id = parseInt(detalleInput.value, 10);

        if (isNaN(id) || id < 1) {
            detalleInput.focus();
            return;
        }

        window.location.href = window.detalleBaseUrl + '/' + id;
    }

    if (detalleInput && detalleBtn) {
        detalleInput.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                openOrderDetail();
            }
        });

        detalleBtn.addEventListener('click', openOrderDetail);
    }

    var searchInput = document.getElementById('filtro-cola');
    var clearButton = document.getElementById('limpiar-filtros');
    var queueBody = document.getElementById('queue-body');
    var queueData = window.pedidosColaData || [];

    var PIN_ICON = '<path d="M12 21s-7-5.5-7-11a7 7 0 0 1 14 0c0 5.5-7 11-7 11z"/><circle cx="12" cy="10" r="2.5"/>';
    var TRUCK_ICON = '<path d="M1 8h14v9H1z"/><path d="M15 11h4l3 3v3h-7"/><circle cx="6" cy="19" r="2"/><circle cx="17" cy="19" r="2"/>';
    var CHECK_ICON = '<path d="M20 6L9 17l-5-5"/>';

    function buildSvg(width, height, innerHtml, strokeWidth) {
        var svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.setAttribute('class', 'icon');
        svg.setAttribute('width', width);
        svg.setAttribute('height', height);
        svg.setAttribute('viewBox', '0 0 24 24');
        svg.setAttribute('fill', 'none');
        svg.setAttribute('stroke', 'currentColor');
        svg.setAttribute('stroke-width', strokeWidth || 2);
        svg.setAttribute('stroke-linecap', 'round');
        svg.setAttribute('stroke-linejoin', 'round');
        svg.setAttribute('aria-hidden', 'true');
        svg.innerHTML = innerHtml;
        return svg;
    }

    function createEl(tag, className, text) {
        var node = document.createElement(tag);

        if (className) {
            node.className = className;
        }

        if (text !== undefined) {
            node.appendChild(document.createTextNode(text));
        }

        return node;
    }

    function cellWith(tdClass, children) {
        var td = document.createElement('td');

        if (tdClass) {
            td.className = tdClass;
        }

        children.forEach(function (child) {
            td.appendChild(child);
        });

        return td;
    }

    function buildPriorityPill(isHigh) {
        var pill = createEl('span', 'priority-pill' + (isHigh ? ' alta' : ' baja'));
        pill.appendChild(createEl('span', 'dot'));
        pill.appendChild(document.createTextNode(isHigh ? 'ALTA' : 'BAJA'));
        return pill;
    }

    function buildDeliveryType(isExpress) {
        var pill = createEl('span', 'delivery-type ' + (isExpress ? 'express' : 'gratis'));
        pill.appendChild(buildSvg(16, 16, isExpress ? TRUCK_ICON : CHECK_ICON, isExpress ? 2 : 2.5));
        pill.appendChild(document.createTextNode(isExpress ? 'Express' : 'Gratis'));
        return pill;
    }

    function buildQueueRow(order, index) {
        var isHigh = order.PriorityLevel === 1;
        var row = document.createElement('tr');

        row.appendChild(cellWith(null, [createEl('span', 'turno-badge', '#' + ('0' + (index + 1)).slice(-2))]));

        var customerCell = createEl('div', 'customer-cell');
        customerCell.appendChild(createEl('strong', null, order.CustomerFullName || ''));
        customerCell.appendChild(createEl('span', 'customer-id', 'Pedido #' + order.OrderId));
        row.appendChild(cellWith(null, [customerCell]));

        row.appendChild(createEl('td', null, order.CustomerPhone || ''));

        var deliveryCell = createEl('span', 'delivery-cell');
        deliveryCell.appendChild(buildSvg(15, 15, PIN_ICON));
        deliveryCell.appendChild(document.createTextNode(order.DeliveryAddress || ''));
        row.appendChild(cellWith(null, [deliveryCell]));

        row.appendChild(createEl('td', 'text-right', 'S/ ' + formatMoney(Number(order.Subtotal) || 0)));

        if (isHigh) {
            row.appendChild(createEl('td', 'text-right', 'S/ ' + formatMoney(Number(order.ShippingCost) || 0)));
        } else {
            row.appendChild(cellWith('text-right', [createEl('span', 'ship-free', 'GRATIS')]));
        }

        row.appendChild(createEl('td', 'text-right total-cell', 'S/ ' + formatMoney(Number(order.Total) || 0)));

        row.appendChild(cellWith(null, [buildPriorityPill(isHigh)]));
        row.appendChild(cellWith(null, [buildDeliveryType(isHigh)]));

        var dispatchButton = document.createElement('button');
        dispatchButton.type = 'button';
        dispatchButton.className = 'btn-dispatch';
        dispatchButton.appendChild(document.createTextNode('Despachar'));
        dispatchButton.addEventListener('click', function () {
            dispatchOrder(order, dispatchButton);
        });
        row.appendChild(cellWith(null, [dispatchButton]));

        return row;
    }

    function dispatchOrder(order, button) {
        if (button.disabled) {
            return;
        }

        button.disabled = true;

        fetch('/api/pedidos/despachar/' + order.OrderId, { method: 'POST' })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('No se pudo despachar el pedido.');
                }

                return response.json();
            })
            .then(function () {
                var row = button.closest('tr');
                row.classList.add('row-dispatch-out');

                setTimeout(function () {
                    queueData = queueData.filter(function (item) {
                        return item.OrderId !== order.OrderId;
                    });
                    window.pedidosColaData = queueData;
                    filterAndRender();
                }, 300);
            })
            .catch(function () {
                button.disabled = false;
                alert('No se pudo despachar el pedido. Inténtalo de nuevo.');
            });
    }

    function renderPackingQueue(pedidos) {
        if (!queueBody) {
            return;
        }

        var countElement = document.getElementById('queue-count');

        if (countElement) {
            countElement.textContent = String(pedidos.length);
        }

        queueBody.innerHTML = '';

        if (!pedidos.length) {
            var emptyTd = createEl('td', 'fila-vacia', 'No hay pedidos en la cola');
            emptyTd.colSpan = 10;
            var emptyRow = document.createElement('tr');
            emptyRow.appendChild(emptyTd);
            queueBody.appendChild(emptyRow);
            return;
        }

        pedidos.forEach(function (order, index) {
            queueBody.appendChild(buildQueueRow(order, index));
        });
    }

    function filterAndRender() {
        if (!searchInput) {
            return;
        }

        var query = searchInput.value.trim().toLowerCase();

        var filtered = query === '' ? queueData : queueData.filter(function (order) {
            var haystack = ((order.CustomerFullName || '') + ' ' + (order.DeliveryAddress || '')).toLowerCase();
            return haystack.indexOf(query) !== -1;
        });

        renderPackingQueue(filtered);
    }

    if (searchInput) {
        searchInput.addEventListener('input', filterAndRender);
    }

    if (clearButton) {
        clearButton.addEventListener('click', function () {
            if (!searchInput) {
                return;
            }

            searchInput.value = '';
            filterAndRender();
        });
    }

    renderPackingQueue(queueData);
});