(function () {
    'use strict';

    const root = document.getElementById('posTerminal');
    if (!root) return;

    const API = root.dataset.apiBase;
    const TOKEN_URL = root.dataset.tokenUrl;
    const SERIE_DEFECTO = 'A';

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
        returnButton: document.getElementById('posReturn'),
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
        mixedMethodWrap: document.getElementById('posMixedMethodWrap'),
        mixedMethod: document.getElementById('posMixedMethod'),
        mixedAmount: document.getElementById('posMixedAmount'),
        reference: document.getElementById('posReference'),
        cash: document.getElementById('posCash'),
        change: document.getElementById('posChange'),
        checkout: document.getElementById('posCheckout'),
        returnModal: document.getElementById('posReturnModal'),
        returnNumber: document.getElementById('posReturnNumber'),
        returnLoad: document.getElementById('posReturnLoad'),
        returnHint: document.getElementById('posReturnHint'),
        returnLines: document.getElementById('posReturnLines'),
        returnReason: document.getElementById('posReturnReason'),
        returnMethod: document.getElementById('posReturnMethod'),
        returnTotal: document.getElementById('posReturnTotal'),
        returnSubmit: document.getElementById('posReturnSubmit')
    };

    const returnState = { invoice: null };
    const serie = () => (state.config && state.config.serie) || SERIE_DEFECTO;

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

    function puede(codigo) {
        const roles = (state.user && state.user.roles) || [];
        if (roles.indexOf('MASTER') >= 0 || roles.indexOf('ADMINISTRADOR') >= 0) return true;
        const permisos = (state.user && state.user.permisos) || [];
        return permisos.indexOf(codigo) >= 0;
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

        // El JWT del POS caduca (60 min por defecto) mientras la cookie de sesión sigue viva:
        // al recibir 401 se renueva el token con /POS/Token y se repite la operación una vez.
        if (res.status === 401 && !opts.retried) {
            await loadToken();
            return api(path, Object.assign({}, opts, { retried: true }));
        }

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
        el.returnButton.disabled = !(state.session && state.session.estado === 'ABIERTA') || !puede('procesar_devolucion');
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
                descuento: 0,
                stock: stock
            });
        }

        renderCart();
        el.search.value = '';
        el.results.innerHTML = '';
        el.search.focus();
    }

    function lineImporte(line) {
        return round2(line.cantidad * line.precio * (1 - (Number(line.descuento) || 0) / 100));
    }

    function cartTotal() {
        return round2(state.cart.reduce(function (sum, l) { return sum + lineImporte(l); }, 0));
    }

    function renderCart() {
        el.cartBody.innerHTML = '';

        if (!state.cart.length) {
            el.cartBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-4">Sin artículos</td></tr>';
        } else {
            state.cart.forEach(function (line, index) {
                const imp = lineImporte(line);
                const descuento = Number(line.descuento) || 0;
                const tr = document.createElement('tr');
                tr.innerHTML =
                    '<td class="ps-3">' +
                        '<div class="fw-bold text-truncate" style="max-width: 200px;">' + esc(line.nombre) + '</div>' +
                        '<small class="text-muted">' + esc(line.codigo) + ' · ' + money(line.precio) + '</small>' +
                    '</td>' +
                    '<td class="text-center">' +
                        '<div class="d-inline-flex align-items-center gap-1">' +
                            '<button type="button" class="btn btn-light btn-icon-sm" data-action="dec" data-index="' + index + '">−</button>' +
                            '<input type="number" class="form-control form-control-sm pos-qty" data-action="qty" data-index="' + index + '" value="' + line.cantidad + '" min="1" step="1" />' +
                            '<button type="button" class="btn btn-light btn-icon-sm" data-action="inc" data-index="' + index + '">+</button>' +
                        '</div>' +
                    '</td>' +
                    '<td class="text-center">' +
                        '<input type="number" class="form-control form-control-sm pos-discount" data-action="discount" data-index="' + index + '" value="' + descuento + '" min="0" max="100" step="1"' +
                        (puede('aplicar_descuento') ? '' : ' disabled title="Sin permiso para aplicar descuentos"') + ' />' +
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

    function mixedTransfer() {
        const total = cartTotal();
        const cash = Number(el.cash.value) || 0;
        return round2(Math.max(0, total - Math.min(cash, total)));
    }

    function updateChange() {
        const total = cartTotal();
        const metodo = paymentMethod();
        const conEfectivo = metodo === 'EFECTIVO' || metodo === 'MIXTO';
        const cash = Number(el.cash.value) || 0;
        const change = round2(cash - total);
        el.change.textContent = money(change);
        el.change.classList.toggle('text-danger', change < -0.001 && metodo === 'EFECTIVO');
        el.change.classList.toggle('text-success', change >= -0.001 && cash > 0 && metodo === 'EFECTIVO');
        el.cashFields.classList.toggle('d-none', !conEfectivo);
        el.transferFields.classList.toggle('d-none', metodo === 'EFECTIVO');
        el.mixedMethodWrap.classList.toggle('d-none', metodo !== 'MIXTO');
        el.mixedAmount.textContent = money(mixedTransfer());
        updateCheckout();
    }

    function updateCheckout() {
        const total = cartTotal();
        const metodo = paymentMethod();
        const cash = Number(el.cash.value) || 0;
        const pagoEfectivoOk = metodo !== 'EFECTIVO' || cash + 0.001 >= total;
        const canSell = !state.busy
            && state.cart.length > 0
            && !!state.session
            && state.session.estado === 'ABIERTA'
            && pagoEfectivoOk;
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
        const referencia = el.reference.value.trim() || null;
        if (total <= 0) { fail('Agregue artículos a la venta.'); return; }

        const pagos = [];
        if (metodo === 'EFECTIVO') {
            const cash = round2(Number(el.cash.value) || 0);
            if (cash + 0.001 < total) { fail('El efectivo recibido no cubre el total.'); return; }
            pagos.push({ formaPago: 'EFECTIVO', monto: cash, vueltoEntregado: round2(cash - total) });
        } else if (metodo === 'MIXTO') {
            const cash = round2(Number(el.cash.value) || 0);
            const resto = round2(Math.max(0, total - Math.min(cash, total)));
            if (cash > 0) pagos.push({ formaPago: 'EFECTIVO', monto: cash, vueltoEntregado: round2(Math.max(0, cash - total)) });
            if (resto > 0) pagos.push({ formaPago: el.mixedMethod.value, monto: resto, referenciaExterna: referencia });
            if (!pagos.length) { fail('Indique el efectivo o el importe a cobrar por transferencia.'); return; }
        } else {
            pagos.push({ formaPago: metodo, monto: total, referenciaExterna: referencia });
        }

        setBusy(true);
        const lineas = state.cart.map(function (l) {
            return {
                productoId: l.productoId,
                cantidad: l.cantidad,
                precioUnitario: l.precio,
                descuentoPorcentaje: Number(l.descuento) || 0,
                impuestoPorcentaje: 0
            };
        });

        try {
            const siguiente = await api('/ventas/siguiente-numero?dispositivoPosId=' + encodeURIComponent(state.config.dispositivoPosId) + '&serie=' + encodeURIComponent(serie()));
            const fecha = new Date().toISOString();
            const venta = {
                entidadId: state.config.entidadId,
                sucursalId: state.config.sucursalId,
                clienteId: state.config.clienteConsumidorFinalId,
                almacenId: state.config.almacenId,
                tipoMovimientoId: state.config.tipoMovimientoVenta,
                numeroFactura: siguiente.numeroFactura,
                serie: serie(),
                fecha: fecha,
                moneda: state.config.monedaBase || 'CUP',
                lineas: lineas,
                pagos: pagos
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
                printTicket(venta, total, pagos);
                resetSale();
                await loadSession();
                toast('success', 'Venta ' + venta.serie + '-' + venta.numeroFactura + ' registrada');
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Venta en cola',
                    html: 'La venta quedó registrada y se procesará en segundo plano.<br><small class="text-muted">' + esc((procesada && procesada.mensajeError) || '') + '</small>'
                });
                resetSale();
                await loadSession();
            }
        } catch (e) {
            fail(e.message);
        } finally {
            setBusy(false);
        }
    }

    function resetSale() {
        state.cart = [];
        el.cash.value = '';
        el.reference.value = '';
        el.payments.querySelector('input[value="EFECTIVO"]').checked = true;
        renderCart();
    }

    function printTicket(venta, total, pagos) {
        const cfg = state.config;
        const rows = state.cart.map(function (line) {
            const descuento = Number(line.descuento) || 0;
            return '<tr><td>' + esc(line.nombre) +
                   (descuento > 0 ? ' <small>(-' + descuento + '%)</small>' : '') + '</td>' +
                   '<td class="r">' + line.cantidad + '</td>' +
                   '<td class="r">' + money(line.precio) + '</td>' +
                   '<td class="r">' + money(lineImporte(line)) + '</td></tr>';
        }).join('');

        const pagosRows = pagos.map(function (p) {
            return '<tr><td>' + esc(PAYMENT_LABEL[p.formaPago] || p.formaPago) + '</td><td class="r">' + money(p.monto) + '</td></tr>' +
                   (p.vueltoEntregado > 0 ? '<tr><td>Cambio</td><td class="r">' + money(p.vueltoEntregado) + '</td></tr>' : '') +
                   (p.referenciaExterna ? '<tr><td>Ref.</td><td class="r">' + esc(p.referenciaExterna) + '</td></tr>' : '');
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
            pagosRows +
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

    function openReturns() {
        if (el.returnButton.disabled) return;
        returnState.invoice = null;
        el.returnNumber.value = '';
        el.returnLines.innerHTML = '';
        el.returnReason.value = '';
        el.returnMethod.value = 'EFECTIVO';
        el.returnTotal.textContent = money(0);
        el.returnSubmit.disabled = true;
        el.returnHint.textContent = 'Cargue la factura para ver sus artículos y cantidades devueltas.';
        bootstrap.Modal.getOrCreateInstance(el.returnModal).show();
        setTimeout(function () { el.returnNumber.focus(); }, 300);
    }

    async function loadReturnInvoice() {
        const numero = el.returnNumber.value.trim();
        if (!numero) { el.returnHint.textContent = 'Escriba el número de factura.'; return; }
        if (!state.config) return;

        el.returnHint.textContent = 'Cargando…';
        try {
            const factura = await api('/sincronizacion/facturas?dispositivoPosId=' + encodeURIComponent(state.config.dispositivoPosId) +
                '&serie=' + encodeURIComponent(serie()) + '&numero=' + encodeURIComponent(numero));
            returnState.invoice = factura;
            if (!factura.lineas.length) {
                el.returnLines.innerHTML = '';
                el.returnHint.textContent = 'La factura no tiene artículos.';
                el.returnSubmit.disabled = true;
                return;
            }
            el.returnHint.textContent = 'Factura ' + factura.serie + '-' + factura.numeroFactura + ' · ' + money(factura.total) + ' (' + factura.estado + ')';
            renderReturnLines();
        } catch (e) {
            returnState.invoice = null;
            el.returnLines.innerHTML = '';
            el.returnSubmit.disabled = true;
            el.returnHint.textContent = e.message;
        }
    }

    function renderReturnLines() {
        const factura = returnState.invoice;
        el.returnLines.innerHTML = '';
        factura.lineas.forEach(function (linea) {
            const restante = round2(linea.cantidad - linea.cantidadDevuelta);
            const tr = document.createElement('tr');
            tr.innerHTML =
                '<td>' + esc(linea.producto) + '</td>' +
                '<td class="text-end">' + esc(linea.cantidad) + '</td>' +
                '<td class="text-end">' + esc(linea.cantidadDevuelta) + '</td>' +
                '<td class="text-end">' + money(linea.precioUnitario) + '</td>' +
                '<td class="text-center">' +
                    '<input type="number" class="form-control form-control-sm text-end" data-return="' + linea.facturaDetalleId + '"' +
                    ' min="0" max="' + restante + '" step="0.001" value="0"' + (restante <= 0 ? ' disabled' : '') + ' />' +
                '</td>';
            el.returnLines.appendChild(tr);
        });
        updateReturnTotal();
    }

    function updateReturnTotal() {
        const factura = returnState.invoice;
        if (!factura) { el.returnTotal.textContent = money(0); el.returnSubmit.disabled = true; return; }

        let total = 0;
        factura.lineas.forEach(function (linea) {
            const input = el.returnLines.querySelector('input[data-return="' + linea.facturaDetalleId + '"]');
            if (!input || input.disabled) return;
            const maximo = round2(linea.cantidad - linea.cantidadDevuelta);
            const cantidad = Math.min(Math.max(Number(input.value) || 0, 0), maximo);
            if (cantidad <= 0) return;
            const proporcion = linea.cantidad > 0 ? cantidad / linea.cantidad : 0;
            const neto = round2(linea.subtotalLinea * proporcion);
            const impuesto = round2(neto * linea.impuestoPorcentaje / 100);
            total += neto + impuesto;
        });

        el.returnTotal.textContent = money(round2(total));
        el.returnSubmit.disabled = total <= 0 || state.busy;
    }

    async function submitReturn() {
        const factura = returnState.invoice;
        if (!factura) return;
        const motivo = el.returnReason.value.trim();
        if (!motivo) { toast('warning', 'Escriba el motivo de la devolución'); return; }

        const lineas = [];
        factura.lineas.forEach(function (linea) {
            const input = el.returnLines.querySelector('input[data-return="' + linea.facturaDetalleId + '"]');
            if (!input || input.disabled) return;
            const maximo = round2(linea.cantidad - linea.cantidadDevuelta);
            const cantidad = Math.min(Math.max(Number(input.value) || 0, 0), maximo);
            if (cantidad > 0) lineas.push({ facturaDetalleId: linea.facturaDetalleId, cantidad: cantidad });
        });
        if (!lineas.length) { toast('warning', 'Indique las cantidades a devolver'); return; }

        el.returnSubmit.disabled = true;
        const texto = el.returnSubmit.textContent;
        el.returnSubmit.textContent = 'Procesando…';
        try {
            const resultado = await api('/sincronizacion/devoluciones', {
                method: 'POST',
                body: JSON.stringify({
                    dispositivoPosId: state.config.dispositivoPosId,
                    sesionCajaPosId: state.session.id,
                    serie: factura.serie,
                    numeroFactura: factura.numeroFactura,
                    motivo: motivo,
                    formaPagoReembolso: el.returnMethod.value,
                    lineas: lineas
                })
            });
            bootstrap.Modal.getOrCreateInstance(el.returnModal).hide();
            Swal.fire({
                icon: 'success',
                title: 'Devolución registrada',
                html: 'Factura <b>' + esc(resultado.serie + '-' + resultado.numeroFactura) + '</b><br>' +
                      'Reembolso: <b>' + money(resultado.totalDevuelto) + '</b> (' + esc(PAYMENT_LABEL[resultado.formaPagoReembolso] || resultado.formaPagoReembolso) + ')'
            });
            await loadSession();
        } catch (e) {
            fail(e.message);
        } finally {
            el.returnSubmit.textContent = texto;
            updateReturnTotal();
        }
    }

    function bindEvents() {
        el.device.addEventListener('change', function () { selectDevice(el.device.value); });
        el.openShift.addEventListener('click', openShift);
        el.closeShift.addEventListener('click', closeShift);
        el.payments.addEventListener('change', function () {
            updateChange();
            if (paymentMethod() === 'EFECTIVO' || paymentMethod() === 'MIXTO') el.cash.focus();
            else el.reference.focus();
        });
        el.mixedMethod.addEventListener('change', updateChange);
        el.returnButton.addEventListener('click', openReturns);
        el.returnLoad.addEventListener('click', loadReturnInvoice);
        el.returnNumber.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); loadReturnInvoice(); }
        });
        el.returnLines.addEventListener('input', updateReturnTotal);
        el.returnSubmit.addEventListener('click', submitReturn);
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
            const input = e.target.closest('input[data-action]');
            if (!input) return;
            const index = Number(input.dataset.index);
            const line = state.cart[index];
            if (!line) return;

            if (input.dataset.action === 'discount') {
                let descuento = Number(input.value) || 0;
                if (descuento < 0) descuento = 0;
                if (descuento > 100) { descuento = 100; toast('warning', 'El descuento máximo es 100%'); }
                line.descuento = descuento;
                renderCart();
                return;
            }

            if (input.dataset.action !== 'qty') return;
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
