class Table {

    constructor(e, t) {
        (this.table = e),
            (this.id = e.id || null),
            (this.initialized = false),
            (this.parentInstance = t),
            (this.thead = e.querySelector("thead")),
            (this.tbody = e.querySelector("tbody")),
            (this.headers = []),
            this.thead &&
            (this.headers = Array.from(this.thead.querySelectorAll("th"))),
            (this.rows = Array.from(this.tbody.querySelectorAll("tr"))),
            (this.filteredRows = [...this.rows]),
            (this.rowsPerPage = parseInt(
                e.getAttribute(this.parentInstance.rowsPerPageAttribute) ??
                this.parentInstance.rowsPerPage
            )),
            (this.currentPage = this.parentInstance.currentPage),
            (this.checkAllCheckBox = null),
            (this.searchInput = null),
            (this.pagination = null),
            (this.paginationInfo = null),
            (this.filters = []),
            (this.rangeFilters = []),
            (this.itemNotFoundMessage = "Sin datos..."),
            (this.deleteUrl = e.querySelector(this.parentInstance.deleteUrl)?.dataset?.tableDeleteUrl ?? "/default/eliminar"),
            (this.deleteRowMessage =  "Está seguro(a) que quiere eliminar selección? Esta acción es irreversible!"),
            (this.deleteMultipleRowsMessage = "Está seguro(a) que quiere eliminar selección? Esta acción es irreversible!");      
  }

   get totalPages() {
    return Math.ceil(this.filteredRows.length / this.rowsPerPage) || 1;
  }

  init() {
    if (!this.initialized) {
      this.setupCheckAll();
      this.setupCheckboxListeners();
      this.setupSearch();
      this.setupFilters();
      this.setupSort();
      this.setupRowsPerPage();
      this.setupPagination();
      this.setupPaginationInfo();
      this.deleteSelected();     
      this.deleteRow();     
      this.initialized = true;
    }
    this.update(); // siempre repinta
    }

  fullRefresh(newRows) {
    // 1) asigna datos
    this.rows = newRows;
    this.filteredRows = [...newRows];
    this.currentPage = 1;

    // 2) ¡vuelve a montar todos los behaviors!
      if (!this.initialized) {
          this.setupCheckAll();
          this.setupCheckboxListeners();
          this.setupSearch();
          this.setupFilters();
          this.setupSort();
          this.setupRowsPerPage();
          this.setupPagination();
          this.setupPaginationInfo();
          this.deleteSelected();       
          this.deleteRow();  
          this.initialized = true;
      }
      this.update();
  }

  setupCheckAll() {
        this.thead &&
            ((this.checkAllCheckBox = this.thead.querySelector(
                this.parentInstance.checkAllSelector
            )),
                this.checkAllCheckBox) &&
            this.checkAllCheckBox.addEventListener("click", (t) => {
                var e = this.rows.map((e) => e.querySelector('input[type="checkbox"]'));
                e && 0 < e.length && e.forEach((e) => (e.checked = t.target.checked));
            });
  }
  setupCheckboxListeners() {
        let t = this.table.querySelectorAll('input[type="checkbox"]');
        this.checkAllCheckBox &&
            0 < t.length &&
            t.forEach((e) => {
                e.addEventListener("change", () => {
                    var e = Array.from(t).some((e) => e.checked);
                    this.deleteSelectedSelector &&
                        0 < this.filteredRows.length &&
                        this.deleteSelectedSelector.classList.toggle("d-none", !e);
                });
            });
  }
  setupSearch() {
    (this.searchInput = this.table.querySelector(
      this.parentInstance.searchSelector
    )),
      this.searchInput &&
        this.searchInput.addEventListener("keyup", (e) => {
          let t = e.target.value.toLowerCase();
          (this.filteredRows = this.rows.filter((e) =>
            Array.from(e.querySelectorAll("td")).some((e) =>
              e.textContent.toLowerCase().includes(t)
            )
          )),
            (this.currentPage = 1),
            this.update();
        });
  }
  setupFilters() {
    (this.filters = Array.from(
      this.table.querySelectorAll(this.parentInstance.filterSelector)
    )),
      this.filters.forEach((e) => {
        e.addEventListener("change", () => {
          this.applyFilters(), (this.currentPage = 1), this.update();
        });
      }),
      (this.rangeFilters = Array.from(
        this.table.querySelectorAll(this.parentInstance.rangeFilterSelector)
      )),
      this.rangeFilters.forEach((e) => {
        e.addEventListener("change", () => {
          this.applyFilters(), (this.currentPage = 1), this.update();
        });
      });
  }
  applyFilters() {
    let t = (s) =>
        this.filters.every((e) => {
          var t = e.value;
          if (!t || "All" === t) return !0;
          let a = e.dataset.tableFilter;
          var e = this.headers.findIndex((e) => e.dataset.column === a);
          return (
            -1 === e ||
            !(e = s.children[e]) ||
            e.textContent.trim().toLowerCase() === t.toLowerCase()
          );
        }),
      a = (h) =>
        this.rangeFilters.every((a) => {
          var s = a.value;
          if (!s || "All" === s) return !0;
          let t = a.dataset.tableRangeFilter;
          a = this.headers.findIndex((e) => e.dataset.column === t);
          if (-1 === a) return !0;
          a = h.children[a];
          if (!a) return !0;
          var e,
            r,
            i,
            a = a.textContent.trim();
          if (!t.toLowerCase().includes("date"))
            return (
              (e = parseFloat(a.replace(/[^\d.-]/g, ""))),
              !isNaN(e) &&
                (s.endsWith("+")
                  ? parseFloat(s.slice(0, -1)) <= e
                  : (([r, i] = s.split("-").map((e) => parseFloat(e.trim()))),
                    !isNaN(r) && !isNaN(i) && r <= e && e <= i))
            );
          {
            var l = new Date(a);
            if (isNaN(l.getTime())) return !1;
            var n = new Date(),
              o = new Date(n.getFullYear(), n.getMonth(), n.getDate()),
              c = new Date(n.getFullYear(), n.getMonth(), n.getDate() + 1);
            let e, t;
            switch (s) {
              case "Today":
                return o <= l && l < c;
              case "Last 7 Days":
                return (
                  (e = new Date(n)).setDate(n.getDate() - 7),
                  (t = c),
                  l >= e && l < t
                );
              case "Last 30 Days":
                return (
                  (e = new Date(n)).setDate(n.getDate() - 30),
                  (t = c),
                  l >= e && l < t
                );
              case "This Year":
                return (
                  (e = new Date(n.getFullYear(), 0, 1)),
                  (t = new Date(n.getFullYear() + 1, 0, 1)),
                  l >= e && l < t
                );
              default:
                return !0;
            }
          }
        });
    this.filteredRows = this.rows.filter((e) => t(e) && a(e));
  }
  setupRowsPerPage() {
    let a = this.table.querySelector(this.parentInstance.rowsPerPageSelector);
    var e;
    a &&
      ((e = Array.from(a.options).map((e) => parseInt(e.value))).includes(
        this.rowsPerPage
      ) || e.push(this.rowsPerPage),
      e.sort((e, t) => e - t),
      (a.innerHTML = ""),
      e.forEach((e) => {
        var t = document.createElement("option");
        (t.value = e.toString()),
          (t.textContent = e.toString()),
          a.appendChild(t);
      }),
      (a.value = this.rowsPerPage),
      a.addEventListener("change", (e) => {
        e.preventDefault(),
          (this.rowsPerPage = parseInt(e.target.value)),
          this.update();
      }));
  }
  setupPagination() {
    this.pagination = this.table.querySelector(
      this.parentInstance.paginationSelector
    );
  }
  renderTablePage(e) {
    this.tbody.innerHTML = "";
    var t,
      e = (e - 1) * this.rowsPerPage,
      a = e + this.rowsPerPage,
      e = this.filteredRows.slice(e, a),
      a = this.tbody.querySelector(".no-results");
    a && a.remove(),
      0 === e.length
        ? ((a = this.table.querySelectorAll("thead th").length),
          ((t = document.createElement("tr")).className = "no-results"),
          (t.innerHTML = `<td colspan="${a}" class="text-center text-muted py-3">${this.itemNotFoundMessage}</td>`),
          this.tbody.appendChild(t))
        : e.forEach((e) => {
            this.tbody.appendChild(e);
          });
  }
  renderPagination() {
    var e = document.createElement("ul"),
      a =
        ((e.className =
          "pagination pagination-boxed mb-0 justify-content-center"),
        (this.pagination.innerHTML = ""),
        this.totalPages),
      t = document.createElement("li");
    (t.className = "page-item " + (1 === this.currentPage ? "disabled" : "")),
      (t.innerHTML =
        '<a href="#" class="page-link"><i class="ti ti-chevron-left"></i></a>'),
      t.addEventListener("click", (e) => {
        e.preventDefault(),
          1 < this.currentPage && (this.currentPage--, this.update());
      }),
      e.appendChild(t);
    for (let t = 1; t <= a; t++) {
      var s = document.createElement("li");
      (s.className = "page-item " + (this.currentPage === t ? "active" : "")),
        (s.innerHTML = `<a href="#" class="page-link">${t}</a>`),
        s.addEventListener("click", (e) => {
          e.preventDefault(), (this.currentPage = t), this.update();
        }),
        e.appendChild(s);
    }
    t = document.createElement("li");
    (t.className = "page-item " + (this.currentPage === a ? "disabled" : "")),
      (t.innerHTML =
        '<a href="#" class="page-link"><i class="ti ti-chevron-right"></i></a>'),
      t.addEventListener("click", (e) => {
        e.preventDefault(),
          this.currentPage < this.totalPages &&
            (this.currentPage++, this.update());
      }),
      e.appendChild(t),
      this.pagination.appendChild(e),
      this.setupPaginationInfo();
  }
  setupPaginationInfo() {
    var e, t, a;
    (this.paginationInfo = this.table.querySelector(
      this.parentInstance.paginationInfoSelector
    )),
      this.paginationInfo &&
        ((this.paginationInfo.className = "text-muted"),
        (e = this.paginationInfo.getAttribute(
          this.parentInstance.paginationInfoAttribute
        )),
        (t = (this.currentPage - 1) * this.rowsPerPage + 1),
        (a = Math.min(
          this.currentPage * this.rowsPerPage,
          this.filteredRows.length
        )),
        (this.paginationInfo.innerHTML =
          `Mostrando <span class="fw-semibold">${t}</span> a <span class="fw-semibold">${a}</span> de <span class="fw-semibold">${this.filteredRows.length}</span> ` +
          (e && "" !== e ? e : "entries")));
  }
  update() {
      var e;
      this.renderTablePage(this.currentPage),
          this.pagination &&
          ((e = 0 < this.filteredRows.length),
              (this.pagination.style.display = e ? "block" : "none"),
              this.paginationInfo &&
              (this.paginationInfo.style.display = e ? "block" : "none"),
              e) &&
          this.renderPagination();
    }
  getTableById(id) {
        return this.tables.find(t => t.id === id);
  }
  setupSort() {
    this.table
      .querySelectorAll(this.parentInstance.sortSelector)
      .forEach((t) => {
        t.style.cursor = "pointer";
        let l = t.querySelector("i");
        l ||
          (((l = document.createElement("i")).className =
            "ti ti-arrows-sort fs-xs ms-1"),
          t.appendChild(l)),
          t.addEventListener("click", (e) => {
            e.preventDefault();
            let r = Array.from(t.parentElement.children).indexOf(t);
            if (-1 !== r) {
              let a = t.getAttribute(this.parentInstance.sortAttribute);
              let s = "asc" === t.dataset.direction ? "desc" : "asc";
              function i(t, a, s) {
                t = t.children[a];
                if (!t) return "";
                let e = "";
                s = (e = (e = s
                  ? (a = t.querySelector(`[data-sort="${s}"]`))
                    ? a.textContent.trim()
                    : ""
                  : (
                      Array.from(t.childNodes).find(
                        (e) =>
                          e.nodeType === Node.TEXT_NODE && e.textContent.trim()
                      ) || t
                    ).textContent.trim()).replace(/\s+/g, " ")).match(
                  /^\(([\d,.\s]+)\)$/
                );
                if (s)
                  return (
                    (a = parseFloat(s[1].replace(/,/g, ""))),
                    isNaN(a) ? e.toLowerCase() : -a
                  );
                if (/^-?[\d,.]+%$/.test(e))
                  return (
                    (t = parseFloat(e.replace("%", ""))), isNaN(t) ? 0 : t / 100
                  );
                s = e.match(/^-?[\$€₹]?\s*([\d,.]+)\s*([KMB])?$/i);
                if (s) {
                  let e = parseFloat(s[1].replace(/,/g, ""));
                  (a = s[2]?.toUpperCase()), (t = { K: 1e3, M: 1e6, B: 1e9 });
                  if (!isNaN(e)) return a && t[a] && (e *= t[a]), e;
                }
                s = e.match(/^(\d+)(st|nd|rd|th)$/i);
                return s
                  ? parseInt(s[1], 10)
                  : /^-?\d+(\.\d+)?$/.test(e)
                  ? ((t = parseFloat(e)), isNaN(t) ? e.toLowerCase() : t)
                  : ((a = new Date(e)),
                    isNaN(a.getTime()) ? e.toLowerCase() : a.getTime());
              }
              "asc" == (t.dataset.direction = s)
                ? (l.className = "ti ti-arrow-up fs-xs ms-1")
                : (l.className = "ti ti-arrow-down fs-xs ms-1"),
                this.filteredRows.sort((e, t) => {
                  (e = i(e, r, a)), (t = i(t, r, a));
                  return "number" == typeof e && "number" == typeof t
                    ? "asc" == s
                      ? e - t
                      : t - e
                    : "string" == typeof e && "string" == typeof t
                    ? "asc" == s
                      ? e.localeCompare(t)
                      : t.localeCompare(e)
                    : 0;
                }),
                (this.currentPage = 1),
                this.update();
            }
          });
      });
  }
  deleteSelected() {
       this.deleteSelectedSelector = this.table.querySelector(this.parentInstance.deleteSelectedSelector);
       if (!this.deleteSelectedSelector) return;
      this.deleteSelectedSelector.addEventListener("click", (e) => {
            e.preventDefault();        
            let seleccionados = this.rows.filter(row =>
                row.querySelector('input[type="checkbox"]').checked
            );

            if (seleccionados.length === 0) {
                Swal.fire({
                    icon: 'info',
                    title: 'Sin selección',
                    text: 'Debes marcar al menos una fila para eliminar.',
                    confirmButtonText: 'Entendido'
                });
                return;
            }

            let { title, text } = seleccionados.length > 1
                ? { title: '¿Eliminar varias filas?', text: 'Esta acción no se puede deshacer.' }
                : { title: '¿Eliminar fila?', text: 'Esta acción no se puede deshacer.' };

            Swal.fire({
                title,
                text,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, eliminar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                reverseButtons: false,

            }).then((result) => {
                if (!result.isConfirmed) return;
                
                let ids = seleccionados.filter(row => row.querySelector('input[type="checkbox"]')?.checked)
                    .map(row => parseInt(row.id))
                    .filter(id => !isNaN(id));                   

                $.ajax(
                    {
                    url: this.deleteUrl,
                    type: 'POST',
                    data: { ids },
                    traditional: true,
                        success: (res) =>
                        {
                        if (res.success) {

                            seleccionados.forEach(row => row.remove());
                            let restantes = this.rows.filter(row => !seleccionados.includes(row));
                            this.rows = [...restantes];
                            this.filteredRows = [...restantes];

                            if (this.currentPage > this.totalPages)
                                this.currentPage = this.totalPages;

                            this.update();
                            this.deleteSelectedSelector.classList.add("d-none");
                            this.checkAllCheckBox.checked = false;

                            Swal.fire({
                                icon: 'success',
                                title: 'Eliminadas',
                                text: 'Las filas fueron eliminadas correctamente.',
                                timer: 1500,
                                showConfirmButton: false
                            });

                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: res.message || 'No se pudo eliminar correctamente.'
                            });
                        }
                    },
                    error: () => {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error del servidor',
                            text: 'Ocurrió un problema al procesar la solicitud.'
                        });
                    }
                });
            });
        });
    }
  deleteRow() {
        var e = this.table.querySelectorAll(this.parentInstance.deleteRowSelector);
        e &&
            e.forEach((r) => {
                r.addEventListener("click", (e) => {
                    e.preventDefault();
                    let { modal: a, confirmButton: t } = this.alert(),
                        s = this.rows;
                    a.show(),
                        t.addEventListener("click", () => {
                            let t = r.closest("tr");
                            t.remove();
                            var e = s.filter((e) => e !== t);
                            (this.rows = [...e]),
                                (this.filteredRows = [...e]),
                                this.currentPage > this.totalPages &&
                                (this.currentPage = this.totalPages),
                                this.update(),
                                a.hide();
                        });
                });
            });
    }
  alert(e) {
        (e = `
        <div id="confirm-delete-modal" class="modal fade" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Cofirmación de eliminación</h4>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">   
                        <p class="mb-0">${e ?? this.deleteRowMessage}</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancelar</button>
                          <button type="button" class="btn btn-danger" id="confirm-delete">Eliminar</button>
                    </div>
                </div>
            </div>
        </div>`),
            document.body.insertAdjacentHTML("beforeend", e),
            (e = document.getElementById("confirm-delete-modal"));
        return {
            modal: new bootstrap.Modal(e),
            confirmButton: e.querySelector("#confirm-delete"),
        };
    }
}
class CustomTable {
  constructor({
    tableSelector: e = "[data-table]",
    checkAllSelector: t = "[data-table-select-all]",
    searchSelector: a = "[data-table-search]",
    filterSelector: s = "select[data-table-filter], input[data-table-filter]",
    rangeFilterSelector: r = "select[data-table-range-filter], input[data-table-range-filter]",
    rowsPerPageSelector: i = "[data-table-set-rows-per-page]",
    rowsPerPageAttribute: l = "data-table-rows-per-page",
    paginationSelector: n = "[data-table-pagination]",
    sortSelector: o = "[data-table-sort]",
    sortAttribute: c = "data-table-sort",
    paginationInfoSelector: h = "[data-table-pagination-info]",
    paginationInfoAttribute: d = "data-table-pagination-info",
    deleteSelectedSelector: u = "[data-table-delete-selected]",
    deleteRowSelector: p = "[data-table-delete-row]",
    deleteUrl: m = "[data-table-delete-url]",  // 🆕 Ruta dinámica
    rowsPerPage: g = 10,
    currentPage: f = 1,
  } = {}) {
    (this.tableSelector = e),
      (this.checkAllSelector = t),
      (this.searchSelector = a),
      (this.filterSelector = s),
      (this.rangeFilterSelector = r),
      (this.rowsPerPageSelector = i),
      (this.rowsPerPageAttribute = l),
      (this.paginationSelector = n),
      (this.sortSelector = o),
      (this.sortAttribute = c),
      (this.paginationInfoSelector = h),
      (this.paginationInfoAttribute = d),
      (this.deleteSelectedSelector = u),
      (this.deleteRowSelector = p),
      (this.deleteUrl = m), // ✅ Guardar ruta dinámica
      (this.rowsPerPage = g),
      (this.currentPage = f),
      (this.tables = []),
      this.init();
  }
  init() {
    document.querySelectorAll(this.tableSelector).forEach((e) => {
      e = new Table(e, this);
      this.tables.push(e), e.init();
    });
  }
}


