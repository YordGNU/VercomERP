class OperChartByTipo {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const tipos = this.iData.map(item => item.tipo || "Sin Tipo");
        const importes = this.iData.map(item => item.importe.toFixed(2));
        const cantidades = this.iData.map(item => item.cantidad.toFixed(2));     

        return {
            series: [
                {
                    name: 'Importe',
                    type: 'column',
                    data: importes
                },
                {
                    name: 'Cantidad',
                    type: 'column',
                    data: cantidades
                }
            ],
            chart: { type: 'bar', height: 350 },          
            title: {
                text: this.titulo,
                align: 'left',
                offsetX: 40
            },
            xaxis: {
                categories: tipos
            },
            yaxis: [
                {
                    seriesName: 'Importe',
                    min: 0,
                    axisTicks: { show: true },
                    axisBorder: { show: true, color: '#008FFB' },
                    labels: {
                        style: { colors: '#008FFB' },
                        formatter: function (value) {
                            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                            if (value >= 1000) return (value / 1000).toFixed(1) + 'K';
                            return value.toFixed(0);
                        }
                    },
                    title: {
                        text: "Importe ($)",
                        style: { color: '#008FFB' }
                    },
                    tooltip: { enabled: true }
                },
                {
                    seriesName: 'Cantidad',
                    opposite: true,
                    min: 0,
                    axisTicks: { show: true },
                    axisBorder: { show: true, color: '#00E396' },
                    labels: {
                        style: { colors: '#00E396' },
                        formatter: function (value) {
                            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                            if (value >= 1000) return (value / 1000).toFixed(1) + 'K';
                            return value.toFixed(0);
                        }
                    },
                    title: {
                        text: "Cantidad (u)",
                        style: { color: '#00E396' }
                    }
                }
            ]        
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }


    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class OperChartByFecha {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {

        const fechas = this.iData.map(item => {
            const partes = item.FechaTexto.trim().split(" ");
            if (partes.length !== 2) return null;

            const [fecha, hora] = partes;
            const [d, m, y] = fecha.split("/");
            if (!d || !m || !y) return null;

            const iso = `${y}-${m.padStart(2, '0')}-${d.padStart(2, '0')}T${hora}`;
            const dateObj = new Date(iso);
            return isNaN(dateObj) ? null : dateObj.toISOString();
        });
       
        const fachanwq = this.iData.map(item => item.FechaTexto);
        const importes = this.iData.map(item => item.Importe);
        const cantidades = this.iData.map(item => item.Cantidad);  
        return {
            series: [
                { name: 'Importe', type: 'column', data: importes },
                { name: 'Cantidad', type: 'column', data: cantidades }
            ],
            chart: { type: 'bar', height: 350 },
            dataLabels: { enabled: !1 },
            theme: {
                palette: 'palette1',
                monochrome: {
                    enabled: false,
                    color: '#255aee',
                    shadeTo: 'light',
                    shadeIntensity: 0.65
                },
            },
            title: {
                text: this.titulo,
                align: 'left',
                offsetX: 40
            },
            xaxis: {
                categories: fachanwq
            },
            yaxis: [
                {
                    seriesName: 'Importe',
                    min: 0,
                    axisTicks: { show: true },
                    axisBorder: { show: true, color: '#008FFB' },
                    labels: {
                        style: { colors: '#008FFB' },
                        formatter: function (value) {
                            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                            if (value >= 1000) return (value / 1000).toFixed(1) + 'K';
                            return value.toFixed(0);
                        }
                    },
                    title: {
                        text: "Importe ($)",
                        style: { color: '#008FFB' }
                    },
                    tooltip: { enabled: true }
                },
                {
                    seriesName: 'Cantidad',
                    opposite: true,
                    min: 0,
                    axisTicks: { show: true },
                    axisBorder: { show: true, color: '#00E396' },
                    labels: {
                        style: { colors: '#00E396' },
                        formatter: function (value) {
                            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                            if (value >= 1000) return (value / 1000).toFixed(1) + 'K';
                            return value.toFixed(0);
                        }
                    },
                    title: {
                        text: "Cantidad (u)",
                        style: { color: '#00E396' }
                    }
                }
            ]       
        };
    }

    render() {
        this.chart = new ApexCharts(document.querySelector(this.containerId), this.getChartOptions());
        this.chart.render();
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class OperChartByPunto{
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const puntos = this.iData.map(item => item.PuntoVenta);
        const importes = this.iData.map(item => item.importe);
        const cantidades = this.iData.map(item => item.cantidad);
        const porcentajes = this.iData.map(item => parseFloat(item.porciento));
        return {
            series: [
                {
                    name: 'Importe',
                    type: 'column',
                    data: importes
                },
                {
                    name: 'Cantidad',
                    type: 'column',
                    data: cantidades
                }
            ],
            chart: { type: 'bar', height: 350 },
            dataLabels: { enabled: !1 },
            title: {
                text: this.titulo,
                align: 'left',
                offsetX: 40
            },
            xaxis: {
                categories: puntos
            },
            yaxis: [
                {
                    seriesName: 'Importe',
                    min: 0,
                    axisTicks: { show: true },
                    axisBorder: { show: true, color: '#008FFB' },
                    labels: {
                        style: { colors: '#008FFB' },
                        formatter: function (value) {
                            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                            if (value >= 1000) return (value / 1000).toFixed(1) + 'K';
                            return value.toFixed(0);
                        }
                    },
                    title: {
                        text: "Importe ($)",
                        style: { color: '#008FFB' }
                    },
                    tooltip: { enabled: true }
                },
                {
                    seriesName: 'Cantidad',
                    opposite: true,
                    min: 0,
                    axisTicks: { show: true },
                    axisBorder: { show: true, color: '#00E396' },
                    labels: {
                        style: { colors: '#00E396' },
                        formatter: function (value) {
                            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                            if (value >= 1000) return (value / 1000).toFixed(1) + 'K';
                            return value.toFixed(0);
                        }
                    },
                    title: {
                        text: "Cantidad (u)",
                        style: { color: '#00E396' }
                    }
                }
            ]        
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class OperChartByPuntoDonnut {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const puntos = this.iData.map(item => item.PuntoVenta);
        const porcentajes = this.iData.map(item => parseFloat(item.porciento.toFixed(2)));
        return {
            chart: {
                height: 320,
                type: "donut",
                dropShadow: {
                    enabled: !0,
                    color: "#111",
                    top: -1,
                    left: 3,
                    blur: 3,
                    opacity: 0.2,
                },
            },           
            stroke: { show: false, width: 0 },
            series: porcentajes,
            labels: puntos,
            dataLabels: { enabled: true },
            theme: {              
                palette: 'palette1',
                monochrome: {
                    enabled: false,
                    color: '#255aee',
                    shadeTo: 'light',
                    shadeIntensity: 0.65
                },
            },
            fill: {
                type: 'gradient',
                gradient: {
                    shade: 'dark',
                    type: "horizontal",
                    shadeIntensity: 0.5,
                    gradientToColors: undefined, // optional, if not defined - uses the shades of same color in series
                    inverseColors: true,
                    opacityFrom: 1,
                    opacityTo: 1,
                    stops: [0, 50, 100],
                    colorStops: []
                }
            },
            states: { hover: { enabled: !1 } },
            legend: {
                show: !0,
                position: "bottom",
                horizontalAlign: "center",
                verticalAlign: "middle",
                floating: !1,
                fontSize: "12px",            
            }, responsive: [
                {
                    breakpoint: 600,
                    options: { chart: { height: 240 }, legend: { show: !1 } },
                },
            ],   
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class OperChartByProducto {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const producto = this.iData.map(item => item.Producto);    
        const porcentajes = this.iData.map(item => parseFloat(item.porciento.toFixed(2)));
        return {
            series: porcentajes,
            chart: { height: 500, type: "donut" },         
            legend: {
                show: !0,
                position: "top",
                horizontalAlign: "center",
                verticalAlign: "middle",
                floating: !1,
                fontSize: "12px",
                offsetX: 0,
                offsetY: 5,
            },
            labels: producto,      
            dataLabels: { enabled: !0, style: { fontSize: "12px", fontWeight: 500 } },
            responsive: [
                {
                    breakpoint: 600,
                    options: { chart: { height: 240 }, legend: { show: !1 } },
                },
            ],
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}

//venta mayorista
class MayChartByCliente {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const clientes = this.iData.map(item => item.Cliente);
        const porcentajes = this.iData.map(item => parseFloat(item.PorcentajeParticipacion.toFixed(2)));
        return {
            chart: {
                height: 320,
                type: "donut",               
                dropShadow: {
                    enabled: !0,
                    color: "#111",
                    top: -1,
                    left: 3,
                    blur: 3,
                    opacity: 0.2,
                },
            },
            title: {
                text: this.titulo,
                align: 'center',             
            },
            stroke: { show: false, width: 0 },
            series: porcentajes,
            labels: clientes,
            dataLabels: { enabled: true },
            theme: {
                palette: 'palette1',
                monochrome: {
                    enabled: false,
                    color: '#255aee',
                    shadeTo: 'light',
                    shadeIntensity: 0.65
                },
            },
            fill: {
                type: 'gradient',
                gradient: {
                    shade: 'dark',
                    type: "horizontal",
                    shadeIntensity: 0.5,
                    gradientToColors: undefined, // optional, if not defined - uses the shades of same color in series
                    inverseColors: true,
                    opacityFrom: 1,
                    opacityTo: 1,
                    stops: [0, 50, 100],
                    colorStops: []
                }
            },
            states: { hover: { enabled: !1 } },
            legend: {                
                show: !0,
                position: "bottom",
                horizontalAlign: "center",
                verticalAlign: "middle",
                floating: !1,
                fontSize: "12px",
            }, responsive: [
                {
                    breakpoint: 600,
                    options: { chart: { height: 240 }, legend: { show: !1 } },
                },
            ],
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class MayChartByTCliente {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const tclientes = this.iData.map(item => item.TipoCliente);
        const porcentajes = this.iData.map(item => parseFloat(item.PorcentajeParticipacion.toFixed(2)));
        return {
            chart: {
                height: 320,
                type: "donut",
                dropShadow: {
                    enabled: !0,
                    color: "#111",
                    top: -1,
                    left: 3,
                    blur: 3,
                    opacity: 0.2,
                },
            },
            title: {
                text: this.titulo,
                align: 'center',
            },
            stroke: { show: false, width: 0 },
            series: porcentajes,
            labels: tclientes,
            dataLabels: { enabled: true },
            theme: {
                palette: 'palette1',
                monochrome: {
                    enabled: false,
                    color: '#255aee',
                    shadeTo: 'light',
                    shadeIntensity: 0.65
                },
            },
            fill: {
                type: 'gradient',
                gradient: {
                    shade: 'dark',
                    type: "horizontal",
                    shadeIntensity: 0.5,
                    gradientToColors: undefined, // optional, if not defined - uses the shades of same color in series
                    inverseColors: true,
                    opacityFrom: 1,
                    opacityTo: 1,
                    stops: [0, 50, 100],
                    colorStops: []
                }
            },
            states: { hover: { enabled: !1 } },
            legend: {
                show: !0,
                position: "bottom",
                horizontalAlign: "center",
                verticalAlign: "middle",
                floating: !1,
                fontSize: "12px",
            }, responsive: [
                {
                    breakpoint: 600,
                    options: { chart: { height: 240 }, legend: { show: !1 } },
                },
            ],
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class MayChartByFOperacion {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const foperacions = this.iData.map(item => item.FormaOperacion);
        const porcentajes = this.iData.map(item => parseFloat(item.PorcentajeParticipacion.toFixed(2)));
        return {
            chart: {
                height: 320,
                type: "donut",
                dropShadow: {
                    enabled: !0,
                    color: "#111",
                    top: -1,
                    left: 3,
                    blur: 3,
                    opacity: 0.2,
                },
            },
            title: {
                text: this.titulo,
                align: 'center',
            },
            stroke: { show: false, width: 0 },
            series: porcentajes,
            labels: foperacions,
            dataLabels: { enabled: true },
            theme: {
                palette: 'palette1',
                monochrome: {
                    enabled: false,
                    color: '#255aee',
                    shadeTo: 'light',
                    shadeIntensity: 0.65
                },
            },
            fill: {
                type: 'gradient',
                gradient: {
                    shade: 'dark',
                    type: "horizontal",
                    shadeIntensity: 0.5,
                    gradientToColors: undefined, // optional, if not defined - uses the shades of same color in series
                    inverseColors: true,
                    opacityFrom: 1,
                    opacityTo: 1,
                    stops: [0, 50, 100],
                    colorStops: []
                }
            },
            states: { hover: { enabled: !1 } },
            legend: {
                show: !0,
                position: "bottom",
                horizontalAlign: "center",
                verticalAlign: "middle",
                floating: !1,
                fontSize: "12px",
            }, responsive: [
                {
                    breakpoint: 600,
                    options: { chart: { height: 240 }, legend: { show: !1 } },
                },
            ],
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}
class MayChartByMPago {
    constructor({ containerId, titulo, iData }) {
        this.containerId = containerId;
        this.titulo = titulo;
        this.iData = iData;
        this.chart = null;
    }

    getChartOptions() {
        const mpago = this.iData.map(item => item.MedioPago);
        const porcentajes = this.iData.map(item => parseFloat(item.PorcentajeParticipacion.toFixed(2)));
        return {
            chart: {
                height: 320,
                type: "donut",
                dropShadow: {
                    enabled: !0,
                    color: "#111",
                    top: -1,
                    left: 3,
                    blur: 3,
                    opacity: 0.2,
                },
            },
            title: {
                text: this.titulo,
                align: 'center',
            },
            stroke: { show: false, width: 0 },
            series: porcentajes,
            labels: mpago,
            dataLabels: { enabled: true },
            theme: {
                palette: 'palette1',
                monochrome: {
                    enabled: false,
                    color: '#255aee',
                    shadeTo: 'light',
                    shadeIntensity: 0.65
                },
            },
            fill: {
                type: 'gradient',
                gradient: {
                    shade: 'dark',
                    type: "horizontal",
                    shadeIntensity: 0.5,
                    gradientToColors: undefined, // optional, if not defined - uses the shades of same color in series
                    inverseColors: true,
                    opacityFrom: 1,
                    opacityTo: 1,
                    stops: [0, 50, 100],
                    colorStops: []
                }
            },
            states: { hover: { enabled: !1 } },
            legend: {
                show: !0,
                position: "bottom",
                horizontalAlign: "center",
                verticalAlign: "middle",
                floating: !1,
                fontSize: "12px",
            }, responsive: [
                {
                    breakpoint: 600,
                    options: { chart: { height: 240 }, legend: { show: !1 } },
                },
            ],
        };
    }

    render() {
        const container = document.querySelector(this.containerId);
        if (!container) {
            console.warn(`Contenedor ${this.containerId} no encontrado`);
            return;
        }

        // Destruir gráfico anterior en el contenedor si quedó huérfano
        if (container._chartInstance && typeof container._chartInstance.destroy === 'function') {
            container._chartInstance.destroy();
        }

        this.chart = new ApexCharts(container, this.getChartOptions());
        this.chart.render();
        container._chartInstance = this.chart;
    }

    destroy() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }

    update(newData, newTitle = null) {
        this.iData = newData;
        if (newTitle) this.titulo = newTitle;
        this.destroy();
        this.render();
    }
}