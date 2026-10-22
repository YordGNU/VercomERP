(function () {
    'use strict';

    const root = document.getElementById('posTerminal');
    if (!root) return;

    const API = root.dataset.apiBase;
    const TOKEN_URL = root.dataset.tokenUrl;
    const SERIE = 'A';

    const state = {
        token: null,
        user: null,
        devices: [],
        config: null,
        session: null,
        cart: [],
        busy: false,
        searchSeq: 0
    };

    const el = {
        device: document.getElementById('posDevice'),
        shiftState: document.getElementById('posShiftState'),
        openShift: document.getElementById('posOpenShift'),
        closeShift: document.getElementById('posCloseShift'),
        search: document.getElementById('posSearch'),
        clearSearch: document.getElementById('posClearSearch'),
        results: document.getElementById('posResults'),
        hint: document.getElementById('posSearchHint'),
        cartBody: document.getElementById('posCartBody'),
        emptyCart: document.getElementById('posEmptyCart'),
        total: document.getElementById('posTotal'),
        payments: document.getElementById('posPayments'),
        cashFields: document.getElementById('posCashFields'),
        transferFields: document.getElementById('posTransferFields'),
        reference: document.getElementById('posReference'),
        cash: document.getElementById('posCash'),
        change: document.getElementById('posChange'),
        checkout: document.getElementById('posCheckout')
    };

    const money = n => (Number(n) || 0).toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const round2 = n => Math.round((Number(n) + Number.EPSILON) * 100) / 100;
    const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    const PAYMENT_LABEL = {
        EFECTIVO: 'Efectivo',
        TRANSFERMOVIL: 'Transfermóvil',
        ENZONA: 'Enzona',
        TRANSFERENCIA_BANCARIA: 'Transferencia bancaria'
    };

    function toast(icon, title) {
        Swal.fire({ toast: true, position: 'top-end', icon: icon, title: title, showConfirmButton: false, timer: 2600, timerProgressBar: true });
    }

    function fail(message) {
        Swal.fire({ icon: 'error', title: 'No se pudo completar', text: message });
    }

    async function api(path, options) {
        const opts = options || {};
        const headers = {
            'Authorization': 'Bearer ' + state.token,
            'Accept': 'application/json'
        };
        if (opts.body) headers['Content-Type'] = 'application/json';

        const res = await fetch(API + path, {
            method: opts.method || 'GET',
            body: opts.body,
            headers: headers
        });

        const text = await res.text();
        let data = null;
        if (text) {
            try { data = JSON.parse(text); } catch (e) { data = text; }
        }

        if (!res.ok) {
            const message = (data && (data.message || data.title)) || (typeof data === 'string' ? data : '') || ('Error HTTP ' + res.status);
            const err = new Error(message);
            err.status = res.status;
            throw err;
        }
        return data;
    }

    async function loadToken() {
        const res = await fetch(TOKEN_URL, { headers: { 'Accept': 'application/json' } });
        const data = await res.json().catch(() => null);
        if (!res.ok || !data || !data.accessToken) {
            throw new Error((data && data.message) || 'No se pudo iniciar el terminal POS.');
        }
        state.user = data;
        state.token = data.accessToken;
    }

    async function loadDevices() {
        state.devices = (await api('/catalogo/dispositivos')) || [];
        el.device.innerHTML = '';

        if (!state.devices.length) {
            el.device.innerHTML = '<option value="">Sin terminales registradas</option>';
            Swal.fire({ icon: 'warning', title: 'No hay terminales POS', html: 'Registre un dispositivo en <b>P. Venta POS &rarr; Dispositivos y Terminales</b>.' });
            return;
        }

        state.devices.forEach(function (d) {
            const option = document.createElement('option');
            option.value = d.id;
            option.textContent = d.codigo + ' · ' + d.nombre + (d.almacenNombre ? ' — ' + d.almacenNombre : '');
            el.device.appendChild(option);
        });

        el.device.disabled = false;
        await selectDevice(state.devices[0].id);
    }

    async function selectDevice(id) {
        if (!id) return;
        el.device.value = id;
        state.cart = [];
        renderCart();
        el.search.value = '';
        el.results.innerHTML = '';
        el.hint.classList.remove('d-none');

        try {
            state.config = await api('/config/terminal?dispositivoPosId=' + encodeURIComponent(id));
        } catch (e) {
            state.config = null;
            fail(e.message);
            return;
        }

        await loadSession();
        el.search.focus();
    }

    async function loadSession() {
        state.session = null;
        if (state.config) {
            try {
                state.session = await api('/caja/sesiones/actual?dispositivoPosId=' + encodeURIComponent(state.config.dispositivoPosId));
            } catch (e) {
                state.session = null;
            }
        }
        renderShift();
    }

    function renderShift() {
        if (state.session && state.session.estado === 'ABIERTA') {
            const otros = round2((state.session.totalTransfermovil || 0) + (state.session.totalEnzona || 0) + (state.session.totalOtrosMedios || 0));
            el.shiftState.innerHTML =
                '<span class="badge bg-success">Turno abierto</span>' +
                '<span class="text-muted small">Ventas: <b>' + money(state.session.totalVentas) + '</b>' +
                ' · ' + (state.session.cantidadFacturas || 0) + ' factura(s)' +
                (otros > 0 ? ' · otros medios: <b>' + money(otros) + '</b>' : '') + '</span>';
            el.openShift.classList.add('d-none');
            el.closeShift.classList.remove('d-none');
        } else {
            el.shiftState.innerHTML =
                '<span class="badge bg-warning">Sin turno</span>' +
                '<span class="text-muted small">Abra la caja para vender</span>';
            el.openShift.classList.remove('d-none');
            el.closeShift.classList.add('d-none');
        }
        updateCheckout();
    }

    async function openShift() {
        if (!state.config) return;
        const result = await Swal.fire({
            title: 'Abrir caja',
            input: 'number',
            inputLabel: 'Fondo de apertura',
            inputValue: 0,
            inputAttributes: { min: 0, step: '0.01' },
            showCancelButton: true,
            confirmButtonText: 'Abrir turno',
            cancelButtonText: 'Cancelar'
        });
        if (!result.isConfirmed) return;

        try {
            await api('/caja/sesiones', {
                method: 'POST',
                body: JSON.stringify({
                    dispositivoPosId: state.config.dispositivoPosId,
                    cajeroId: state.user.userId,
                    montoApertura: Number(result.value) || 0
                })
            });
            toast('success', 'Turno abierto');
            await loadSession();
        } catch (e) {
            fail(e.message);
        }
    }

    async function closeShift() {
        if (!state.session) return;
        const s = state.session;
        const esperado = round2((s.montoApertura || 0) + (s.totalEfectivo || 0));
        const otros = round2((s.totalTransfermovil || 0) + (s.totalEnzona || 0) + (s.totalOtrosMedios || 0));
        const result = await Swal.fire({
            title: 'Cerrar caja',
            html: 'Ventas del turno: <b>' + money(s.totalVentas) + '</b><br>' +
                  'Efectivo esperado en caja: <b>' + money(esperado) + '</b>' +
                  (otros > 0 ? '<br><small class="text-muted">Cobrado por otros medios (fuera de caja): ' + money(otros) + '</small>' : ''),
            input: 'number',
            inputLabel: 'Efectivo declarado',
            inputValue: esperado,
            inputAttributes: { min: 0, step: '0.01' },
            showCancelButton: true,
            confirmButtonText: 'Cerrar turno',
            cancelButtonText: 'Cancelar'
        });
        if (!result.isConfirmed) return;

        try {
            const cierre = await api('/caja/sesiones/' + encodeURIComponent(state.session.id) + '/cierre', {
                method: 'POST',
                body: JSON.stringify({ montoCierreDeclarado: Number(result.value) || 0 })
            });
            const diff = round2((cierre.montoCierreDeclarado || 0) - (cierre.montoCierreSistema || 0));
            Swal.fire({
                icon: Math.abs(diff) < 0.01 ? 'success' : 'warning',
                title: 'Turno cerrado',
                html: 'Sistema: <b>' + money(cierre.montoCierreSistema) + '</b><br>' +
                      'Declarado: <b>' + money(cierre.montoCierreDeclarado) + '</b><br>' +
                      'Diferencia: <b>' + money(diff) + '</b>' +
                      (otros > 0 ? '<br><small class="text-muted">Otros medios del turno: ' + money(otros) + '</small>' : '')
            });
            state.cart = [];
            renderCart();
            await loadSession();
        } catch (e) {
            fail(e.message);
        }
    }

    async function onSearch() {
        const q = el.search.value.trim();
        if (!state.config) return;

        if (q.length < 2) {
            el.results.innerHTML = '';
            el.hint.textContent = 'Escriba al menos 2 caracteres para buscar.';
            el.hint.classList.remove('d-none');
            return;
        }

        const seq = ++state.searchSeq;
        el.hint.textContent = 'Buscando…';
        el.hint.classList.remove('d-none');

        try {
            const items = await api('/catalogo/productos?almacenId=' + encodeURIComponent(state.config.almacenId) +
                '&search=' + encodeURIComponent(q));
            if (seq !== state.searchSeq) return;
            renderResults(items || []);
        } catch (e) {
            if (seq !== state.searchSeq) return;
            el.results.innerHTML = '';
            el.hint.textContent = e.message;
        }
    }

    function renderResults(items) {
        el.results.innerHTML = '';
        if (!items.length) {
            el.hint.textContent = 'Sin coincidencias.';
            el.hint.classList.remove('d-none');
            return;
        }

        el.hint.classList.add('d-none');
        items.slice(0, 20).forEach(function (p) {
            const sinStock = (Number(p.stock) || 0) <= 0;
            const precio = p.precioVentaActual;
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'list-group-item list-group-item-action pos-result-item' + (sinStock ? ' sin-stock' : '');
            button.disabled = sinStock || precio == null;
            button.innerHTML =
                '<div class="d-flex justify-content-between align-items-center gap-2">' +
                    '<div class="overflow-hidden">' +
                        '<div class="fw-bold text-truncate">' + esc(p.nombre) + '</div>' +
                        '<small class="text-muted">' + esc(p.codigo) + ' · existencia: ' + esc(p.stock) + '</small>' +
                    '</div>' +
                    '<div class="text-end">' +
                        '<div class="fw-bold text-primary">' + (precio == null ? 'Sin precio' : money(precio)) + '</div>' +
                        (sinStock ? '<small class="text-danger">Sin existencia</small>' : '') +
                    '</div>' +
                '</div>';
            button.addEventListener('click', function () { addToCart(p); });
            el.results.appendChild(button);
        });
    }

    function addToCart(product) {
        const precio = Number(product.precioVentaActual);
        if (!isFinite(precio) || precio < 0) {
            fail('El producto no tiene precio de venta asignado.');
            return;
        }

        const line = state.cart.find(function (l) { return l.productoId === product.id; });
        const stock = Number(product.stock) || 0;

        if (line) {
            if (line.cantidad + 1 > stock) { toast('warning', 'Existencia insuficiente'); return; }
            line.cantidad += 1;
        } else {
            state.cart.push({
                productoId: product.id,
                codigo: product.codigo,
                nombre: product.nombre,
                precio: precio,
                cantidad: 1,
                stock: stock
            });
        }

        renderCart();
        el.search.value = '';
        el.results.innerHTML = '';
        el.search.focus();
    }

    function cartTotal() {
        return round2(state.cart.reduce(function (sum, l) { return sum + l.cantidad * l.precio; }, 0));
    }

    function renderCart() {
        el.cartBody.innerHTML = '';

        if (!state.cart.length) {
            el.cartBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted py-4">Sin artículos</td></tr>';
        } else {
            state.cart.forEach(function (line, index) {
                const imp = round2(line.cantidad * line.precio);
                const tr = document.createElement('tr');
                tr.innerHTML =
                    '<td class="ps-3">' +
                        '<div class="fw-bold text-truncate" style="max-width: 220px;">' + esc(line.nombre) + '</div>' +
                        '<small class="text-muted">' + esc(line.codigo) + ' · ' + money(line.precio) + '</small>' +
                    '</td>' +
                    '<td class="text-center">' +
                        '<div class="d-inline-flex align-items-center gap-1">' +
                            '<button type="button" class="btn btn-light btn-icon-sm" data-action="dec" data-index="' + index + '">−</button>' +
                            '<input type="number" class="form-control form-control-sm pos-qty" data-action="qty" data-index="' + index + '" value="' + line.cantidad + '" min="1" step="1" />' +
                            '<button type="button" class="btn btn-light btn-icon-sm" data-action="inc" data-index="' + index + '">+</button>' +
                        '</div>' +
                    '</td>' +
                    '<td class="text-end fw-bold">' + money(imp) + '</td>' +
                    '<td class="text-end pe-3">' +
                        '<button type="button" class="btn btn-sm btn-icon btn-light rounded-circle text-danger" data-action="del" data-index="' + index + '" title="Quitar">' +
                            '<i class="ti ti-x"></i>' +
                        '</button>' +
                    '</td>';
                el.cartBody.appendChild(tr);
            });
        }

        el.total.textContent = money(cartTotal());
        updateChange();
        updateCheckout();
    }

    function paymentMethod() {
        const checked = el.payments.querySelector('input[name="posPayment"]:checked');
        return checked ? checked.value : 'EFECTIVO';
    }

    function updateChange() {
        const total = cartTotal();
        const isCash = paymentMethod() === 'EFECTIVO';
        const cash = Number(el.cash.value) || 0;
        const change = round2(cash - total);
        el.change.textContent = money(change);
        el.change.classList.toggle('text-danger', change < -0.001);
        el.change.classList.toggle('text-success', change >= -0.001 && cash > 0);
        el.cashFields.classList.toggle('d-none', !isCash);
        el.transferFields.classList.toggle('d-none', isCash);
        updateCheckout();
    }

    function updateCheckout() {
        const total = cartTotal();
        const isCash = paymentMethod() === 'EFECTIVO';
        const cash = Number(el.cash.value) || 0;
        const canSell = !state.busy
            && state.cart.length > 0
            && !!state.session
            && state.session.estado === 'ABIERTA'
            && (!isCash || cash + 0.001 >= total);
        el.checkout.disabled = !canSell;
    }

    function setBusy(value) {
        state.busy = value;
        el.checkout.innerHTML = value
            ? '<span class="spinner-border spinner-border-sm me-2"></span>Procesando…'
            : '<i class="ti ti-cash-banknote me-1"></i>COBRAR';
        updateCheckout();
    }

    function newIdempotencyKey() {
        if (window.crypto && typeof window.crypto.randomUUID === 'function') return window.crypto.randomUUID();
        return Date.now() + '-' + Math.random().toString(16).slice(2);
    }

    async function checkout() {
        if (state.busy) return;
        if (!state.session || state.session.estado !== 'ABIERTA') { fail('Abra la caja para vender.'); return; }
        if (!state.config) { fail('No hay terminal seleccionada.'); return; }

        const total = cartTotal();
        const metodo = paymentMethod();
        const esEfectivo = metodo === 'EFECTIVO';
        const cash = esEfectivo ? round2(Number(el.cash.value) || 0) : total;
        const vuelto = esEfectivo ? round2(cash - total) : 0;
        const referencia = esEfectivo ? null : (el.reference.value.trim() || null);
        if (total <= 0) { fail('Agregue artículos a la venta.'); return; }
        if (esEfectivo && cash + 0.001 < total) { fail('El efectivo recibido no cubre el total.'); return; }

        setBusy(true);
        const lineas = state.cart.map(function (l) {
            return { productoId: l.productoId, cantidad: l.cantidad, precioUnitario: l.precio, descuentoPorcentaje: 0, impuestoPorcentaje: 0 };
        });

        try {
            const siguiente = await api('/ventas/siguiente-numero?dispositivoPosId=' + encodeURIComponent(state.config.dispositivoPosId) + '&serie=' + SERIE);
            const fecha = new Date().toISOString();
            const venta = {
                entidadId: state.config.entidadId,
                sucursalId: state.config.sucursalId,
                clienteId: state.config.clienteConsumidorFinalId,
                almacenId: state.config.almacenId,
                tipoMovimientoId: state.config.tipoMovimientoVenta,
                numeroFactura: siguiente.numeroFactura,
                serie: SERIE,
                fecha: fecha,
                moneda: state.config.monedaBase || 'CUP',
                lineas: lineas,
                pagos: [esEfectivo
                    ? { formaPago: 'EFECTIVO', monto: cash, vueltoEntregado: vuelto }
                    : { formaPago: metodo, monto: total, referenciaExterna: referencia }]
            };

            const key = newIdempotencyKey();
            await api('/sincronizacion/ventas', {
                method: 'POST',
                body: JSON.stringify({
                    dispositivoPosId: state.config.dispositivoPosId,
                    sesionCajaPosId: state.session.id,
                    idempotencyKey: key,
                    fechaVentaLocal: fecha,
                    venta: venta
                })
            });

            const procesada = await api('/sincronizacion/ventas/' + encodeURIComponent(state.config.dispositivoPosId) + '/' + encodeURIComponent(key) + '/procesar', { method: 'POST' });

            if (procesada && procesada.estado === 'PROCESADO') {
                printTicket(venta, total, { metodo: metodo, monto: cash, vuelto: vuelto, referencia: referencia });
                state.cart = [];
                el.cash.value = '';
                el.reference.value = '';
                el.payments.querySelector('input[value="EFECTIVO"]').checked = true;
                renderCart();
                await loadSession();
                toast('success', 'Venta ' + venta.serie + '-' + venta.numeroFactura + ' registrada');
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Venta en cola',
                    html: 'La venta quedó registrada y se procesará en segundo plano.<br><small class="text-muted">' + esc((procesada && procesada.mensajeError) || '') + '</small>'
                });
                state.cart = [];
                el.reference.value = '';
                el.payments.querySelector('input[value="EFECTIVO"]').checked = true;
                renderCart();
                await loadSession();
            }
        } catch (e) {
            fail(e.message);
        } finally {
            setBusy(false);
        }
    }

    function printTicket(venta, total, pago) {
        const cfg = state.config;
        const rows = state.cart.map(function (line) {
            return '<tr><td>' + esc(line.nombre) + '</td>' +
                   '<td class="r">' + line.cantidad + '</td>' +
                   '<td class="r">' + money(line.precio) + '</td>' +
                   '<td class="r">' + money(round2(line.cantidad * line.precio)) + '</td></tr>';
        }).join('');

        const html = '<!doctype html><html><head><meta charset="utf-8"><title>Ticket ' + esc(venta.serie + '-' + venta.numeroFactura) + '</title>' +
            '<style>' +
            'body{font-family:"Courier New",monospace;font-size:12px;width:76mm;margin:0 auto;color:#000}' +
            'h1{font-size:13px;margin:0;text-align:center}' +
            '.c{text-align:center}.b{font-weight:bold}.mt{margin-top:6px}.hr{border-top:1px dashed #000;margin:6px 0}' +
            'table{width:100%;border-collapse:collapse}td{padding:1px 0;vertical-align:top}' +
            '.r{text-align:right;white-space:nowrap}.tot{font-size:14px;font-weight:bold}' +
            '</style></head><body>' +
            '<h1>' + esc(cfg.entidadNombre) + '</h1>' +
            '<div class="c">NIT: ' + esc(cfg.entidadNit) + '</div>' +
            (cfg.entidadDireccion ? '<div class="c">' + esc(cfg.entidadDireccion) + '</div>' : '') +
            (cfg.entidadTelefono ? '<div class="c">Tel: ' + esc(cfg.entidadTelefono) + '</div>' : '') +
            '<div class="hr"></div>' +
            '<div class="b">FACTURA DE VENTA ' + esc(venta.serie + '-' + venta.numeroFactura) + '</div>' +
            '<div>Fecha: ' + esc(new Date(venta.fecha).toLocaleString('es-ES')) + '</div>' +
            '<div>Terminal: ' + esc(cfg.dispositivoNombre) + '</div>' +
            '<div>Cajero: ' + esc(state.user.nombreCompleto) + '</div>' +
            '<div>Sucursal: ' + esc(cfg.sucursalNombre) + '</div>' +
            '<div class="hr"></div>' +
            '<table><thead><tr><td class="b">Producto</td><td class="b r">Cant</td><td class="b r">Precio</td><td class="b r">Importe</td></tr></thead>' +
            '<tbody>' + rows + '</tbody></table>' +
            '<div class="hr"></div>' +
            '<table>' +
            '<tr class="tot"><td>TOTAL ' + esc(venta.moneda) + '</td><td class="r">' + money(total) + '</td></tr>' +
            '<tr><td>' + esc(PAYMENT_LABEL[pago.metodo] || pago.metodo) + '</td><td class="r">' + money(pago.monto) + '</td></tr>' +
            (pago.metodo === 'EFECTIVO' ? '<tr><td>Cambio</td><td class="r">' + money(pago.vuelto) + '</td></tr>' : '') +
            (pago.referencia ? '<tr><td>Ref.</td><td class="r">' + esc(pago.referencia) + '</td></tr>' : '') +
            '</table>' +
            '<div class="hr"></div>' +
            '<div class="c mt">¡Gracias por su compra!</div>' +
            '<div class="c" style="margin-top:10px;font-size:10px">Documento generado por VercomERP</div>' +
            '</body></html>';

        const win = window.open('', '_blank', 'width=380,height=640');
        if (!win) { toast('info', 'Habilite las ventanas emergentes para imprimir el ticket'); return; }
        win.document.open();
        win.document.write(html);
        win.document.close();
        win.focus();
        setTimeout(function () { win.print(); }, 250);
    }

    function bindEvents() {
        el.device.addEventListener('change', function () { selectDevice(el.device.value); });
        el.openShift.addEventListener('click', openShift);
        el.closeShift.addEventListener('click', closeShift);
        el.payments.addEventListener('change', function () {
            updateChange();
            (paymentMethod() === 'EFECTIVO' ? el.cash : el.reference).focus();
        });
        el.clearSearch.addEventListener('click', function () {
            el.search.value = '';
            el.results.innerHTML = '';
            el.search.focus();
        });

        el.search.addEventListener('input', debounce(onSearch, 250));
        el.search.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const first = el.results.querySelector('button.pos-result-item:not(:disabled)');
                if (first) first.click();
            }
        });

        el.emptyCart.addEventListener('click', function () {
            if (!state.cart.length) return;
            state.cart = [];
            renderCart();
        });

        el.cartBody.addEventListener('click', function (e) {
            const btn = e.target.closest('button[data-action]');
            if (!btn) return;
            const index = Number(btn.dataset.index);
            const line = state.cart[index];
            if (!line) return;

            if (btn.dataset.action === 'inc') {
                if (line.cantidad + 1 > line.stock) { toast('warning', 'Existencia insuficiente'); return; }
                line.cantidad += 1;
            } else if (btn.dataset.action === 'dec') {
                line.cantidad -= 1;
                if (line.cantidad <= 0) state.cart.splice(index, 1);
            } else if (btn.dataset.action === 'del') {
                state.cart.splice(index, 1);
            }
            renderCart();
        });

        el.cartBody.addEventListener('change', function (e) {
            const input = e.target.closest('input[data-action="qty"]');
            if (!input) return;
            const index = Number(input.dataset.index);
            const line = state.cart[index];
            if (!line) return;

            let value = Math.floor(Number(input.value) || 0);
            if (value <= 0) { state.cart.splice(index, 1); renderCart(); return; }
            if (value > line.stock) { value = line.stock; toast('warning', 'Existencia insuficiente'); }
            line.cantidad = value;
            renderCart();
        });

        el.cash.addEventListener('input', updateChange);
        el.checkout.addEventListener('click', checkout);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'F2') { e.preventDefault(); el.search.focus(); el.search.select(); }
            if (e.key === 'Escape' && document.activeElement === el.search) {
                el.search.value = '';
                el.results.innerHTML = '';
            }
        });
    }

    function debounce(fn, wait) {
        let timer = null;
        return function () {
            const args = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () { fn.apply(null, args); }, wait);
        };
    }

    (async function boot() {
        bindEvents();
        try {
            await loadToken();
        } catch (e) {
            fail(e.message);
            return;
        }
        try {
            await loadDevices();
        } catch (e) {
            fail(e.message);
        }
    })();
})();
