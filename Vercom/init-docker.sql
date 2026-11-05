USE [master]
GO

-- Crear la base de datos SIN especificar rutas (usa las de Linux por defecto)
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'VERCOMERP')
BEGIN
    CREATE DATABASE [VERCOMERP]
END
GO

-- Configuración básica compatible con Linux
ALTER DATABASE [VERCOMERP] SET COMPATIBILITY_LEVEL = 150
GO

ALTER DATABASE [VERCOMERP] SET RECOVERY SIMPLE
GO

ALTER DATABASE [VERCOMERP] SET AUTO_CLOSE OFF
GO

ALTER DATABASE [VERCOMERP] SET AUTO_SHRINK OFF
GO

ALTER DATABASE [VERCOMERP] SET AUTO_UPDATE_STATISTICS ON
GO

ALTER DATABASE [VERCOMERP] SET QUERY_STORE = ON
GO

ALTER DATABASE [VERCOMERP] SET QUERY_STORE (OPERATION_MODE = READ_WRITE)
GO

USE [VERCOMERP]
GO

/****** Object:  Schema [comercial]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [comercial]
GO
/****** Object:  Schema [contabilidad]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [contabilidad]
GO
/****** Object:  Schema [integracion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [integracion]
GO
/****** Object:  Schema [inventario]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [inventario]
GO
/****** Object:  Schema [Modelo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [Modelo]
GO
/****** Object:  Schema [nucleo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [nucleo]
GO
/****** Object:  Schema [produccion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [produccion]
GO
/****** Object:  Schema [reportes]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [reportes]
GO
/****** Object:  Schema [rrhh]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE SCHEMA [rrhh]
GO
/****** Object:  Table [contabilidad].[presupuesto]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[presupuesto](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[anio] [smallint] NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[aprobado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[presupuesto_linea]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[presupuesto_linea](
	[id] [uniqueidentifier] NOT NULL,
	[presupuesto_id] [uniqueidentifier] NOT NULL,
	[cuenta_id] [uniqueidentifier] NOT NULL,
	[centro_costo_id] [uniqueidentifier] NULL,
	[mes] [smallint] NOT NULL,
	[monto_planificado] [numeric](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[presupuesto_id] ASC,
	[cuenta_id] ASC,
	[centro_costo_id] ASC,
	[mes] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[asiento_contable]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[asiento_contable](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[periodo_id] [uniqueidentifier] NOT NULL,
	[tipo_comprobante_id] [int] NOT NULL,
	[numero_comprobante] [bigint] NOT NULL,
	[fecha] [date] NOT NULL,
	[concepto] [nvarchar](max) NOT NULL,
	[modulo_origen] [nvarchar](30) NOT NULL,
	[documento_origen_tipo] [nvarchar](50) NULL,
	[documento_origen_id] [uniqueidentifier] NULL,
	[total_debe] [numeric](18, 2) NOT NULL,
	[total_haber] [numeric](18, 2) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[asiento_reversion_id] [uniqueidentifier] NULL,
	[creado_por] [uniqueidentifier] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[tipo_comprobante_id] ASC,
	[numero_comprobante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[asiento_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[asiento_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[asiento_id] [uniqueidentifier] NOT NULL,
	[linea] [smallint] NOT NULL,
	[cuenta_id] [uniqueidentifier] NOT NULL,
	[centro_costo_id] [uniqueidentifier] NULL,
	[tercero_tipo] [nvarchar](20) NULL,
	[tercero_id] [uniqueidentifier] NULL,
	[debe] [numeric](18, 2) NOT NULL,
	[haber] [numeric](18, 2) NOT NULL,
	[glosa] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[asiento_id] ASC,
	[linea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [contabilidad].[v_ejecucion_presupuesto]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [contabilidad].[v_ejecucion_presupuesto] AS
SELECT
    pl.presupuesto_id,
    pl.cuenta_id,
    pl.centro_costo_id,
    pl.mes,
    pl.monto_planificado,
    COALESCE(SUM(ad.debe - ad.haber), 0) AS monto_real
FROM contabilidad.presupuesto_linea pl
JOIN contabilidad.presupuesto p ON p.id = pl.presupuesto_id
LEFT JOIN contabilidad.asiento_contable ac
    ON DATEPART(MONTH, ac.fecha) = pl.mes
    AND DATEPART(YEAR, ac.fecha) = p.anio
    AND ac.entidad_id = p.entidad_id
    AND ac.estado = 'CONTABILIZADO'
LEFT JOIN contabilidad.asiento_detalle ad
    ON ad.asiento_id = ac.id AND ad.cuenta_id = pl.cuenta_id
    AND (ad.centro_costo_id = pl.centro_costo_id OR pl.centro_costo_id IS NULL)
GROUP BY pl.presupuesto_id, pl.cuenta_id, pl.centro_costo_id, pl.mes, pl.monto_planificado;
GO
/****** Object:  Table [nucleo].[auditoria]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[auditoria](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[usuario_id] [uniqueidentifier] NULL,
	[nombre_usuario] [nvarchar](50) NOT NULL,
	[accion] [nvarchar](20) NOT NULL,
	[esquema_tabla] [nvarchar](100) NOT NULL,
	[registro_id] [nvarchar](100) NULL,
	[valores_anteriores] [nvarchar](max) NULL,
	[valores_nuevos] [nvarchar](max) NULL,
	[ip_origen] [nvarchar](45) NULL,
	[canal] [nvarchar](20) NOT NULL,
	[dispositivo_id] [uniqueidentifier] NULL,
	[ocurrido_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [reportes].[v_auditoria_reversiones]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [reportes].[v_auditoria_reversiones] AS
SELECT a.id, a.usuario_id, a.nombre_usuario, a.esquema_tabla, a.registro_id,
       a.valores_anteriores, a.valores_nuevos, a.canal, a.ocurrido_en
FROM nucleo.auditoria a
WHERE a.accion = 'REVERSION';
GO
/****** Object:  View [reportes].[v_auditoria_accesos]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [reportes].[v_auditoria_accesos] AS
SELECT a.id, a.usuario_id, a.nombre_usuario, a.accion, a.ip_origen, a.canal, a.ocurrido_en
FROM nucleo.auditoria a
WHERE a.accion IN ('LOGIN','LOGIN_FALLIDO','LOGOUT');
GO
/****** Object:  Table [contabilidad].[cuenta_contable]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[cuenta_contable](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](200) NOT NULL,
	[cuenta_padre_id] [uniqueidentifier] NULL,
	[nivel] [smallint] NOT NULL,
	[clase] [nvarchar](20) NOT NULL,
	[naturaleza] [nvarchar](10) NOT NULL,
	[acepta_movimiento] [bit] NOT NULL,
	[requiere_centro_costo] [bit] NOT NULL,
	[requiere_tercero] [bit] NOT NULL,
	[moneda] [nvarchar](3) NOT NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[periodo_contable]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[periodo_contable](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[anio] [smallint] NOT NULL,
	[mes] [smallint] NOT NULL,
	[fecha_inicio] [date] NOT NULL,
	[fecha_fin] [date] NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[cerrado_por] [uniqueidentifier] NULL,
	[cerrado_en] [datetimeoffset](7) NULL,
	[creado_en] [datetimeoffset](7) NULL,
	[actualizado_en] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[anio] ASC,
	[mes] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [contabilidad].[v_saldo_cuenta]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [contabilidad].[v_saldo_cuenta] AS
SELECT
    ac.entidad_id,
    ad.cuenta_id,
    c.codigo AS codigo_cuenta,
    c.nombre AS nombre_cuenta,
    c.clase,
    c.naturaleza,
    pc.id AS periodo_id,
    pc.anio,
    pc.mes,
    SUM(ad.debe) AS total_debe,
    SUM(ad.haber) AS total_haber,
    CASE WHEN c.naturaleza = 'DEUDORA'
         THEN SUM(ad.debe) - SUM(ad.haber)
         ELSE SUM(ad.haber) - SUM(ad.debe)
    END AS saldo
FROM contabilidad.asiento_detalle ad
JOIN contabilidad.asiento_contable ac ON ac.id = ad.asiento_id
JOIN contabilidad.cuenta_contable c ON c.id = ad.cuenta_id
JOIN contabilidad.periodo_contable pc ON pc.id = ac.periodo_id
WHERE ac.estado = 'CONTABILIZADO'
GROUP BY ac.entidad_id, ad.cuenta_id, c.codigo, c.nombre, c.clase, c.naturaleza, pc.id, pc.anio, pc.mes;
GO
/****** Object:  Table [integracion].[sesion_caja_pos]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[sesion_caja_pos](
	[id] [uniqueidentifier] NOT NULL,
	[dispositivo_pos_id] [uniqueidentifier] NOT NULL,
	[cajero_id] [uniqueidentifier] NOT NULL,
	[fecha_apertura] [datetimeoffset](7) NOT NULL,
	[monto_apertura] [numeric](14, 2) NOT NULL,
	[fecha_cierre] [datetimeoffset](7) NULL,
	[monto_cierre_declarado] [numeric](14, 2) NULL,
	[monto_cierre_sistema] [numeric](14, 2) NULL,
	[diferencia_arqueo]  AS (CONVERT([numeric](14,2),[monto_cierre_declarado]-[monto_cierre_sistema])) PERSISTED,
	[total_ventas] [numeric](16, 2) NOT NULL,
	[total_efectivo] [numeric](16, 2) NOT NULL,
	[total_transfermovil] [numeric](16, 2) NOT NULL,
	[total_enzona] [numeric](16, 2) NOT NULL,
	[total_otros_medios] [numeric](16, 2) NOT NULL,
	[cantidad_facturas] [int] NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[asiento_cierre_id] [uniqueidentifier] NULL,
	[observaciones_cierre] [nvarchar](max) NULL,
	[supervisor_conciliacion_id] [uniqueidentifier] NULL,
	[caja_id] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[usuario]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[usuario](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[nombre_usuario] [nvarchar](50) NOT NULL,
	[nombre_completo] [nvarchar](150) NOT NULL,
	[email] [nvarchar](150) NULL,
	[hash_password] [nvarchar](255) NOT NULL,
	[debe_cambiar_pass] [bit] NOT NULL,
	[intentos_fallidos] [smallint] NOT NULL,
	[bloqueado_hasta] [datetimeoffset](7) NULL,
	[ultimo_login] [datetimeoffset](7) NULL,
	[activo] [bit] NOT NULL,
	[es_empleado_id] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[actualizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[nombre_usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[caja]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[caja](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[cuenta_contable_id] [uniqueidentifier] NOT NULL,
	[limite_efectivo] [numeric](18, 2) NULL,
	[saldo_actual] [numeric](18, 2) NOT NULL,
	[activa] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [integracion].[dispositivo_pos]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[dispositivo_pos](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NOT NULL,
	[almacen_id] [uniqueidentifier] NOT NULL,
	[api_cliente_id] [uniqueidentifier] NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[identificador_hardware] [nvarchar](100) NULL,
	[caja_id] [uniqueidentifier] NOT NULL,
	[ultima_sincronizacion] [datetimeoffset](7) NULL,
	[version_app_pos] [nvarchar](20) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [contabilidad].[v_arqueo_caja]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE VIEW [contabilidad].[v_arqueo_caja] AS
SELECT
    s.id AS sesion_id,
    s.caja_id,
    c.nombre AS caja_nombre,
    s.dispositivo_pos_id,
    dp.nombre AS dispositivo_nombre,
    s.cajero_id,
    u.nombre_completo AS cajero_nombre,
    s.fecha_apertura,
    s.fecha_cierre,
    s.monto_apertura,
    s.total_efectivo,
    s.monto_cierre_sistema,
    s.monto_cierre_declarado,
    s.diferencia_arqueo,
    s.observaciones_cierre,
    s.supervisor_conciliacion_id,
    s.estado,
    s.asiento_cierre_id,
    a.numero_comprobante AS asiento_numero,
    CASE
        WHEN s.diferencia_arqueo IS NOT NULL AND s.diferencia_arqueo <> 0 AND s.asiento_cierre_id IS NULL THEN 'PENDIENTE'
        WHEN s.asiento_cierre_id IS NOT NULL THEN 'CONTABILIZADO'
        ELSE 'SIN_DIFERENCIA'
    END AS estado_conciliacion
FROM integracion.sesion_caja_pos s
LEFT JOIN contabilidad.caja c ON c.id = s.caja_id
LEFT JOIN integracion.dispositivo_pos dp ON dp.id = s.dispositivo_pos_id
LEFT JOIN nucleo.usuario u ON u.id = s.cajero_id
LEFT JOIN contabilidad.asiento_contable a ON a.id = s.asiento_cierre_id;

GO
/****** Object:  Table [comercial].[cliente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[cliente](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[tipo_persona] [nvarchar](15) NOT NULL,
	[nit_o_ci] [nvarchar](20) NULL,
	[nombre_razon_social] [nvarchar](255) NOT NULL,
	[direccion] [nvarchar](max) NULL,
	[telefono] [nvarchar](30) NULL,
	[email] [nvarchar](150) NULL,
	[segmento] [nvarchar](20) NOT NULL,
	[lista_precio_id] [uniqueidentifier] NULL,
	[limite_credito] [numeric](14, 2) NOT NULL,
	[cuenta_contable_id] [uniqueidentifier] NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [comercial].[contrato_economico]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[contrato_economico](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[tercero_tipo] [nvarchar](15) NOT NULL,
	[cliente_id] [uniqueidentifier] NULL,
	[proveedor_id] [uniqueidentifier] NULL,
	[numero_contrato] [nvarchar](40) NOT NULL,
	[objeto] [nvarchar](max) NOT NULL,
	[fecha_firma] [date] NOT NULL,
	[fecha_inicio] [date] NOT NULL,
	[fecha_fin] [date] NULL,
	[monto_total] [numeric](16, 2) NULL,
	[documento_url] [nvarchar](max) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[fecha_fin_original] [date] NULL,
	[monto_total_original] [numeric](16, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [comercial].[contrato_economico_suplemento]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[contrato_economico_suplemento](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[contrato_id] [uniqueidentifier] NOT NULL,
	[numero_suplemento] [int] NOT NULL,
	[fecha_firma] [date] NOT NULL,
	[fecha_inicio] [date] NULL,
	[fecha_fin] [date] NULL,
	[concepto] [nvarchar](max) NULL,
	[documento_url] [nvarchar](max) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[motivo_anulacion] [nvarchar](max) NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[monto_total_nuevo] [numeric](16, 2) NULL,
 CONSTRAINT [PK_contrato_economico_suplemento] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_suplemento_numero] UNIQUE NONCLUSTERED 
(
	[contrato_id] ASC,
	[numero_suplemento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [comercial].[devolucion_venta]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[devolucion_venta](
	[id] [uniqueidentifier] NOT NULL,
	[factura_id] [uniqueidentifier] NOT NULL,
	[fecha] [date] NOT NULL,
	[motivo] [nvarchar](max) NOT NULL,
	[total_devuelto] [numeric](16, 2) NOT NULL,
	[movimiento_inventario_id] [uniqueidentifier] NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[autorizado_por] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [comercial].[devolucion_venta_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[devolucion_venta_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[devolucion_id] [uniqueidentifier] NOT NULL,
	[factura_detalle_id] [uniqueidentifier] NOT NULL,
	[cantidad_devuelta] [numeric](14, 4) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [comercial].[factura_venta]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[factura_venta](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NOT NULL,
	[numero_factura] [nvarchar](30) NOT NULL,
	[serie] [nvarchar](10) NOT NULL,
	[cliente_id] [uniqueidentifier] NOT NULL,
	[contrato_id] [uniqueidentifier] NULL,
	[almacen_id] [uniqueidentifier] NOT NULL,
	[canal_venta] [nvarchar](20) NOT NULL,
	[tipo_venta] [nvarchar](15) NOT NULL,
	[dispositivo_pos_id] [uniqueidentifier] NULL,
	[sesion_caja_pos_id] [uniqueidentifier] NULL,
	[fecha] [datetimeoffset](7) NOT NULL,
	[subtotal] [numeric](16, 2) NOT NULL,
	[descuento_total] [numeric](16, 2) NOT NULL,
	[impuesto_ventas_total] [numeric](16, 2) NOT NULL,
	[total] [numeric](16, 2) NOT NULL,
	[moneda] [nvarchar](3) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[motivo_anulacion] [nvarchar](max) NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[cuenta_por_cobrar_id] [uniqueidentifier] NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[sucursal_id] ASC,
	[serie] ASC,
	[numero_factura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [comercial].[factura_venta_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[factura_venta_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[factura_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad] [numeric](14, 4) NOT NULL,
	[precio_unitario] [numeric](14, 4) NOT NULL,
	[descuento_porcentaje] [numeric](5, 2) NOT NULL,
	[costo_unitario_venta] [numeric](14, 4) NULL,
	[impuesto_porcentaje] [numeric](5, 2) NOT NULL,
	[subtotal_linea] [numeric](16, 2) NOT NULL,
	[movimiento_inventario_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [comercial].[forma_pago_venta]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[forma_pago_venta](
	[id] [uniqueidentifier] NOT NULL,
	[factura_id] [uniqueidentifier] NOT NULL,
	[forma_pago] [nvarchar](24) NOT NULL,
	[monto] [numeric](16, 2) NOT NULL,
	[referencia_externa] [nvarchar](100) NULL,
	[vuelto_entregado] [numeric](16, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [comercial].[orden_compra]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[orden_compra](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[numero_orden] [nvarchar](30) NOT NULL,
	[proveedor_id] [uniqueidentifier] NOT NULL,
	[contrato_id] [uniqueidentifier] NULL,
	[almacen_destino_id] [uniqueidentifier] NOT NULL,
	[fecha] [date] NOT NULL,
	[fecha_entrega_esperada] [date] NULL,
	[moneda] [nvarchar](3) NOT NULL,
	[subtotal] [numeric](16, 2) NOT NULL,
	[total] [numeric](16, 2) NOT NULL,
	[estado] [nvarchar](20) NOT NULL,
	[aprobado_por] [uniqueidentifier] NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[numero_orden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [comercial].[orden_compra_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[orden_compra_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[orden_compra_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad_solicitada] [numeric](14, 4) NOT NULL,
	[cantidad_recibida] [numeric](14, 4) NOT NULL,
	[precio_unitario] [numeric](14, 4) NOT NULL,
	[subtotal_linea]  AS (CONVERT([numeric](16,2),[cantidad_solicitada]*[precio_unitario])) PERSISTED,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [comercial].[proveedor]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[proveedor](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[tipo_persona] [nvarchar](15) NOT NULL,
	[nit] [nvarchar](20) NULL,
	[razon_social] [nvarchar](255) NOT NULL,
	[direccion] [nvarchar](max) NULL,
	[telefono] [nvarchar](30) NULL,
	[email] [nvarchar](150) NULL,
	[cuenta_bancaria] [nvarchar](40) NULL,
	[cuenta_contable_id] [uniqueidentifier] NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [comercial].[recepcion_compra]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[recepcion_compra](
	[id] [uniqueidentifier] NOT NULL,
	[orden_compra_id] [uniqueidentifier] NOT NULL,
	[movimiento_inventario_id] [uniqueidentifier] NULL,
	[cuenta_por_pagar_id] [uniqueidentifier] NULL,
	[fecha] [date] NOT NULL,
	[numero_informe_recepcion] [nvarchar](30) NULL,
	[recibido_por] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [comercial].[tope_precio_mfp]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [comercial].[tope_precio_mfp](
	[id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NULL,
	[familia_id] [uniqueidentifier] NULL,
	[precio_maximo] [numeric](14, 2) NOT NULL,
	[vigente_desde] [date] NOT NULL,
	[vigente_hasta] [date] NULL,
	[resolucion_referencia] [nvarchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[activo_fijo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[activo_fijo](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[codigo_inventario] [nvarchar](30) NOT NULL,
	[descripcion] [nvarchar](255) NOT NULL,
	[cuenta_activo_id] [uniqueidentifier] NOT NULL,
	[cuenta_depreciacion_id] [uniqueidentifier] NOT NULL,
	[cuenta_gasto_dep_id] [uniqueidentifier] NOT NULL,
	[fecha_adquisicion] [date] NOT NULL,
	[valor_adquisicion] [numeric](18, 2) NOT NULL,
	[valor_residual] [numeric](18, 2) NOT NULL,
	[vida_util_meses] [int] NOT NULL,
	[tasa_depreciacion_anual] [numeric](5, 2) NULL,
	[metodo_depreciacion] [nvarchar](20) NOT NULL,
	[depreciacion_acumulada] [numeric](18, 2) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[fecha_baja] [date] NULL,
	[motivo_baja] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo_inventario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[activo_fijo_depreciacion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[activo_fijo_depreciacion](
	[id] [uniqueidentifier] NOT NULL,
	[activo_fijo_id] [uniqueidentifier] NOT NULL,
	[periodo_id] [uniqueidentifier] NOT NULL,
	[monto] [numeric](18, 2) NOT NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[calculado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[activo_fijo_id] ASC,
	[periodo_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[centro_costo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[centro_costo](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[cuenta_bancaria]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[cuenta_bancaria](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[banco] [nvarchar](100) NOT NULL,
	[numero_cuenta] [nvarchar](40) NOT NULL,
	[tipo_cuenta] [nvarchar](20) NOT NULL,
	[cuenta_contable_id] [uniqueidentifier] NOT NULL,
	[saldo_actual] [numeric](18, 2) NOT NULL,
	[activa] [bit] NOT NULL,
	[moneda] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[numero_cuenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[cuenta_por_cobrar]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[cuenta_por_cobrar](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[cliente_id] [uniqueidentifier] NOT NULL,
	[documento_origen_tipo] [nvarchar](50) NOT NULL,
	[documento_origen_id] [uniqueidentifier] NOT NULL,
	[asiento_origen_id] [uniqueidentifier] NULL,
	[fecha_emision] [date] NOT NULL,
	[fecha_vencimiento] [date] NOT NULL,
	[monto_original] [numeric](18, 2) NOT NULL,
	[saldo_pendiente] [numeric](18, 2) NOT NULL,
	[moneda] [nvarchar](3) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[documento_origen_numero] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[cuenta_por_pagar]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[cuenta_por_pagar](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[proveedor_id] [uniqueidentifier] NOT NULL,
	[documento_origen_tipo] [nvarchar](50) NOT NULL,
	[documento_origen_id] [uniqueidentifier] NOT NULL,
	[asiento_origen_id] [uniqueidentifier] NULL,
	[fecha_emision] [date] NOT NULL,
	[fecha_vencimiento] [date] NOT NULL,
	[monto_original] [numeric](18, 2) NOT NULL,
	[saldo_pendiente] [numeric](18, 2) NOT NULL,
	[moneda] [nvarchar](3) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[documento_origen_numero] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[declaracion_jurada]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[declaracion_jurada](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[tipo_obligacion_id] [int] NOT NULL,
	[periodo_id] [uniqueidentifier] NOT NULL,
	[base_imponible] [numeric](18, 2) NOT NULL,
	[monto_calculado] [numeric](18, 2) NOT NULL,
	[monto_pagado] [numeric](18, 2) NOT NULL,
	[fecha_limite] [date] NOT NULL,
	[fecha_presentacion] [date] NULL,
	[estado] [nvarchar](20) NOT NULL,
	[numero_dj] [nvarchar](40) NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[generado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[tipo_obligacion_id] ASC,
	[periodo_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[movimiento_bancario]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[movimiento_bancario](
	[id] [uniqueidentifier] NOT NULL,
	[cuenta_bancaria_id] [uniqueidentifier] NOT NULL,
	[fecha] [date] NOT NULL,
	[tipo] [nvarchar](10) NOT NULL,
	[monto] [numeric](18, 2) NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[referencia] [nvarchar](100) NULL,
	[conciliado] [bit] NOT NULL,
	[fecha_conciliacion] [date] NULL,
	[asiento_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[pago_aplicado]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[pago_aplicado](
	[id] [uniqueidentifier] NOT NULL,
	[tipo] [nvarchar](10) NOT NULL,
	[cuenta_por_cobrar_id] [uniqueidentifier] NULL,
	[cuenta_por_pagar_id] [uniqueidentifier] NULL,
	[fecha] [date] NOT NULL,
	[monto] [numeric](18, 2) NOT NULL,
	[forma_pago] [nvarchar](24) NOT NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[referencia_externa] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[tipo_comprobante]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[tipo_comprobante](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](10) NOT NULL,
	[nombre] [nvarchar](80) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [contabilidad].[tipo_obligacion_fiscal]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [contabilidad].[tipo_obligacion_fiscal](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[periodicidad] [nvarchar](15) NOT NULL,
	[tasa_actual] [numeric](6, 3) NULL,
	[base_legal] [nvarchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [integracion].[api_cliente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[api_cliente](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[client_id] [nvarchar](64) NOT NULL,
	[client_secret_hash] [nvarchar](255) NOT NULL,
	[scopes] [nvarchar](max) NOT NULL,
	[activo] [bit] NOT NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[revocado_en] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[client_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [integracion].[api_log]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[api_log](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[api_cliente_id] [uniqueidentifier] NULL,
	[metodo_http] [nvarchar](10) NOT NULL,
	[endpoint] [nvarchar](255) NOT NULL,
	[codigo_respuesta] [smallint] NULL,
	[duracion_ms] [int] NULL,
	[ip_origen] [nvarchar](45) NULL,
	[ocurrido_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [integracion].[api_rate_limit]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[api_rate_limit](
	[api_cliente_id] [uniqueidentifier] NOT NULL,
	[solicitudes_por_minuto] [int] NOT NULL,
	[actualizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[api_cliente_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [integracion].[api_token]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[api_token](
	[id] [uniqueidentifier] NOT NULL,
	[api_cliente_id] [uniqueidentifier] NOT NULL,
	[token_hash] [nvarchar](255) NOT NULL,
	[tipo] [nvarchar](15) NOT NULL,
	[emitido_en] [datetimeoffset](7) NOT NULL,
	[expira_en] [datetimeoffset](7) NOT NULL,
	[revocado] [bit] NOT NULL,
	[ip_origen] [nvarchar](45) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[token_hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [integracion].[movimiento_caja_pos]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[movimiento_caja_pos](
	[id] [uniqueidentifier] NOT NULL,
	[sesion_caja_pos_id] [uniqueidentifier] NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[monto] [numeric](14, 2) NOT NULL,
	[factura_id] [uniqueidentifier] NULL,
	[motivo] [nvarchar](max) NULL,
	[autorizado_por] [uniqueidentifier] NULL,
	[ocurrido_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [integracion].[pos_rango_numeracion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[pos_rango_numeracion](
	[id] [uniqueidentifier] NOT NULL,
	[dispositivo_pos_id] [uniqueidentifier] NOT NULL,
	[tipo_documento] [nvarchar](40) NOT NULL,
	[serie] [nvarchar](10) NOT NULL,
	[numero_desde] [bigint] NOT NULL,
	[numero_hasta] [bigint] NOT NULL,
	[numero_siguiente_local] [bigint] NOT NULL,
	[asignado_en] [datetimeoffset](7) NOT NULL,
	[agotado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [integracion].[pos_sync_log]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[pos_sync_log](
	[id] [uniqueidentifier] NOT NULL,
	[dispositivo_pos_id] [uniqueidentifier] NOT NULL,
	[tipo_sync] [nvarchar](20) NOT NULL,
	[direccion] [nvarchar](10) NOT NULL,
	[registros_procesados] [int] NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[detalle_error] [nvarchar](max) NULL,
	[iniciado_en] [datetimeoffset](7) NOT NULL,
	[finalizado_en] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [integracion].[pos_venta_pendiente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[pos_venta_pendiente](
	[id] [uniqueidentifier] NOT NULL,
	[dispositivo_pos_id] [uniqueidentifier] NOT NULL,
	[sesion_caja_pos_id] [uniqueidentifier] NOT NULL,
	[idempotency_key] [nvarchar](80) NOT NULL,
	[payload_json] [nvarchar](max) NOT NULL,
	[fecha_venta_local] [datetimeoffset](7) NOT NULL,
	[fecha_recibido_servidor] [datetimeoffset](7) NOT NULL,
	[estado] [nvarchar](20) NOT NULL,
	[factura_id] [uniqueidentifier] NULL,
	[mensaje_error] [nvarchar](max) NULL,
	[intentos_procesamiento] [smallint] NOT NULL,
	[procesado_en] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[dispositivo_pos_id] ASC,
	[idempotency_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [integracion].[webhook_entrega]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[webhook_entrega](
	[id] [uniqueidentifier] NOT NULL,
	[suscripcion_id] [uniqueidentifier] NOT NULL,
	[payload_json] [nvarchar](max) NOT NULL,
	[intento_numero] [smallint] NOT NULL,
	[codigo_respuesta_http] [smallint] NULL,
	[exitoso] [bit] NOT NULL,
	[proximo_reintento_en] [datetimeoffset](7) NULL,
	[enviado_en] [datetimeoffset](7) NOT NULL,
	[mensaje_error] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [integracion].[webhook_suscripcion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [integracion].[webhook_suscripcion](
	[id] [uniqueidentifier] NOT NULL,
	[api_cliente_id] [uniqueidentifier] NOT NULL,
	[evento] [nvarchar](60) NOT NULL,
	[url_destino] [nvarchar](max) NOT NULL,
	[secreto_firma_hash] [nvarchar](255) NOT NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [inventario].[almacen]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[almacen](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[es_punto_venta] [bit] NOT NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[conteo_fisico]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[conteo_fisico](
	[id] [uniqueidentifier] NOT NULL,
	[almacen_id] [uniqueidentifier] NOT NULL,
	[fecha] [date] NOT NULL,
	[tipo] [nvarchar](15) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[responsable_id] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[conteo_fisico_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[conteo_fisico_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[conteo_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad_sistema] [numeric](16, 4) NOT NULL,
	[cantidad_fisica] [numeric](16, 4) NULL,
	[diferencia]  AS (CONVERT([numeric](16,4),[cantidad_fisica]-[cantidad_sistema])) PERSISTED,
	[justificacion] [nvarchar](max) NULL,
	[movimiento_ajuste_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [inventario].[existencia]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[existencia](
	[id] [uniqueidentifier] NOT NULL,
	[almacen_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad] [numeric](16, 4) NOT NULL,
	[costo_promedio] [numeric](14, 4) NOT NULL,
	[stock_minimo] [numeric](16, 4) NOT NULL,
	[stock_maximo] [numeric](16, 4) NULL,
	[actualizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[almacen_id] ASC,
	[producto_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[existencia_lote]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[existencia_lote](
	[id] [uniqueidentifier] NOT NULL,
	[almacen_id] [uniqueidentifier] NULL,
	[producto_id] [uniqueidentifier] NULL,
	[lote] [varchar](50) NULL,
	[fecha_vencimiento] [datetimeoffset](7) NULL,
	[cantidad] [numeric](18, 0) NULL,
	[actualizado_en] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[familia_producto]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[familia_producto](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[familia_padre_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[lista_precio]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[lista_precio](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[canal] [nvarchar](15) NOT NULL,
	[vigente_desde] [date] NOT NULL,
	[vigente_hasta] [date] NULL,
	[activa] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[lista_precio_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[lista_precio_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[lista_precio_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[precio] [numeric](14, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[lista_precio_id] ASC,
	[producto_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[movimiento_inventario]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[movimiento_inventario](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[tipo_movimiento_id] [int] NOT NULL,
	[numero_documento] [nvarchar](30) NOT NULL,
	[almacen_origen_id] [uniqueidentifier] NULL,
	[almacen_destino_id] [uniqueidentifier] NULL,
	[fecha] [datetimeoffset](7) NOT NULL,
	[referencia_externa_tipo] [nvarchar](50) NULL,
	[referencia_externa_id] [uniqueidentifier] NULL,
	[canal] [nvarchar](15) NOT NULL,
	[dispositivo_pos_id] [uniqueidentifier] NULL,
	[observaciones] [nvarchar](max) NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[numero_documento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [inventario].[movimiento_inventario_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[movimiento_inventario_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[movimiento_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad] [numeric](16, 4) NOT NULL,
	[costo_unitario] [numeric](14, 4) NULL,
	[lote] [nvarchar](50) NULL,
	[fecha_vencimiento] [date] NULL,
	[observaciones] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [inventario].[producto]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[producto](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[codigo_barras] [nvarchar](50) NULL,
	[nombre] [nvarchar](200) NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[familia_id] [uniqueidentifier] NULL,
	[unidad_medida_id] [int] NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[cuenta_inventario_id] [uniqueidentifier] NULL,
	[cuenta_costo_venta_id] [uniqueidentifier] NULL,
	[cuenta_ingreso_id] [uniqueidentifier] NULL,
	[precio_venta_actual] [numeric](14, 2) NULL,
	[aplica_impuesto_ventas] [bit] NOT NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[actualizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [inventario].[tipo_movimiento]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[tipo_movimiento](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[naturaleza] [nvarchar](10) NOT NULL,
	[afecta_costo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [inventario].[unidad_medida]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [inventario].[unidad_medida](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](10) NOT NULL,
	[nombre] [nvarchar](50) NOT NULL,
	[es_fraccionable] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[backup_log]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[backup_log](
	[id] [uniqueidentifier] NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[ruta_archivo] [nvarchar](max) NOT NULL,
	[tamano_bytes] [bigint] NULL,
	[estado] [nvarchar](20) NOT NULL,
	[mensaje_error] [nvarchar](max) NULL,
	[iniciado_en] [datetimeoffset](7) NOT NULL,
	[finalizado_en] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[consecutivo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[consecutivo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[tipo_documento] [nvarchar](40) NOT NULL,
	[serie] [nvarchar](10) NOT NULL,
	[ultimo_numero] [bigint] NOT NULL,
	[longitud_padding] [smallint] NOT NULL,
	[actualizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[sucursal_id] ASC,
	[tipo_documento] ASC,
	[serie] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[entidad]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[entidad](
	[id] [uniqueidentifier] NOT NULL,
	[razon_social] [nvarchar](255) NOT NULL,
	[nombre_comercial] [nvarchar](255) NULL,
	[nit] [nvarchar](20) NOT NULL,
	[codigo_reeup] [nvarchar](20) NULL,
	[forma_juridica] [nvarchar](50) NOT NULL,
	[direccion_legal] [nvarchar](max) NOT NULL,
	[municipio] [nvarchar](100) NULL,
	[provincia] [nvarchar](100) NULL,
	[telefono] [nvarchar](30) NULL,
	[email] [nvarchar](150) NULL,
	[fecha_constitucion] [date] NULL,
	[licencia_actividad] [nvarchar](100) NULL,
	[moneda_base] [nvarchar](3) NOT NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[actualizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo_reeup] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[nit] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[feedback]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[feedback](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[usuario_id] [uniqueidentifier] NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[mensaje] [nvarchar](max) NOT NULL,
	[metadata_tecnica] [nvarchar](max) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[notificacion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[notificacion](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NULL,
	[usuario_id] [uniqueidentifier] NULL,
	[titulo] [nvarchar](200) NOT NULL,
	[mensaje] [nvarchar](max) NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[enlace] [nvarchar](500) NULL,
	[leida] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_notificacion] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[parametro_sistema]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[parametro_sistema](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](80) NOT NULL,
	[valor] [nvarchar](max) NOT NULL,
	[tipo_dato] [nvarchar](20) NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[vigente_desde] [date] NOT NULL,
	[vigente_hasta] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC,
	[vigente_desde] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[permiso]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[permiso](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](80) NOT NULL,
	[modulo] [nvarchar](50) NOT NULL,
	[descripcion] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[rol]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[rol](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[es_sistema] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[entidad_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[rol_permiso]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[rol_permiso](
	[rol_id] [int] NOT NULL,
	[permiso_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[rol_id] ASC,
	[permiso_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[sucursal]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[sucursal](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[tipo] [nvarchar](30) NOT NULL,
	[direccion] [nvarchar](max) NULL,
	[municipio] [nvarchar](100) NULL,
	[provincia] [nvarchar](100) NULL,
	[telefono] [nvarchar](30) NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[latitud] [decimal](10, 6) NULL,
	[longitud] [decimal](10, 6) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [nucleo].[usuario_rol]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [nucleo].[usuario_rol](
	[usuario_id] [uniqueidentifier] NOT NULL,
	[rol_id] [int] NOT NULL,
	[sucursal_id] [uniqueidentifier] NOT NULL,
	[asignado_en] [datetimeoffset](7) NOT NULL,
	[asignado_por] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[usuario_id] ASC,
	[rol_id] ASC,
	[sucursal_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[analisis_desviacion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[analisis_desviacion](
	[id] [uniqueidentifier] NOT NULL,
	[orden_produccion_id] [uniqueidentifier] NOT NULL,
	[componente] [nvarchar](20) NOT NULL,
	[costo_estandar] [numeric](16, 4) NOT NULL,
	[costo_real] [numeric](16, 4) NOT NULL,
	[desviacion]  AS (CONVERT([numeric](16,4),[costo_real]-[costo_estandar])) PERSISTED,
	[analizado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[equipo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[equipo](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[activo_fijo_id] [uniqueidentifier] NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[fecha_ultima_revision] [date] NULL,
	[frecuencia_mantenimiento_dias] [int] NULL,
	[estado] [nvarchar](15) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[ficha_costo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[ficha_costo](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[version] [int] NOT NULL,
	[vigente_desde] [date] NOT NULL,
	[vigente_hasta] [date] NULL,
	[costo_materia_prima] [numeric](14, 4) NOT NULL,
	[costo_mano_obra] [numeric](14, 4) NOT NULL,
	[gastos_indirectos] [numeric](14, 4) NOT NULL,
	[costo_total_unitario]  AS (CONVERT([numeric](14,4),([costo_materia_prima]+[costo_mano_obra])+[gastos_indirectos])) PERSISTED,
	[margen_porcentaje] [numeric](5, 2) NULL,
	[precio_sugerido] [numeric](14, 2) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[producto_id] ASC,
	[version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[lista_materiales]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[lista_materiales](
	[id] [uniqueidentifier] NOT NULL,
	[producto_terminado_id] [uniqueidentifier] NOT NULL,
	[version] [int] NOT NULL,
	[activa] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[producto_terminado_id] ASC,
	[version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[lista_materiales_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[lista_materiales_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[lista_materiales_id] [uniqueidentifier] NOT NULL,
	[producto_insumo_id] [uniqueidentifier] NOT NULL,
	[cantidad_requerida] [numeric](14, 4) NOT NULL,
	[porcentaje_merma] [numeric](5, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[lista_materiales_id] ASC,
	[producto_insumo_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[mantenimiento_programado]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[mantenimiento_programado](
	[id] [uniqueidentifier] NOT NULL,
	[equipo_id] [uniqueidentifier] NOT NULL,
	[fecha_programada] [date] NOT NULL,
	[fecha_ejecutada] [date] NULL,
	[tipo] [nvarchar](15) NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[costo] [numeric](14, 2) NULL,
	[responsable_id] [uniqueidentifier] NULL,
	[estado] [nvarchar](15) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [produccion].[merma]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[merma](
	[id] [uniqueidentifier] NOT NULL,
	[orden_produccion_id] [uniqueidentifier] NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad] [numeric](14, 4) NOT NULL,
	[causa] [nvarchar](30) NOT NULL,
	[valor_contable] [numeric](16, 4) NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[fecha] [date] NOT NULL,
	[observaciones] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [produccion].[orden_produccion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[orden_produccion](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[numero_orden] [nvarchar](30) NOT NULL,
	[producto_terminado_id] [uniqueidentifier] NOT NULL,
	[lista_materiales_id] [uniqueidentifier] NOT NULL,
	[ficha_costo_id] [uniqueidentifier] NULL,
	[almacen_insumos_id] [uniqueidentifier] NOT NULL,
	[almacen_producto_id] [uniqueidentifier] NOT NULL,
	[cantidad_planificada] [numeric](14, 4) NOT NULL,
	[cantidad_producida] [numeric](14, 4) NOT NULL,
	[fecha_inicio_plan] [date] NULL,
	[fecha_fin_plan] [date] NULL,
	[fecha_inicio_real] [datetimeoffset](7) NULL,
	[fecha_fin_real] [datetimeoffset](7) NULL,
	[estado] [nvarchar](20) NOT NULL,
	[costo_real_total] [numeric](16, 4) NULL,
	[asiento_consumo_id] [uniqueidentifier] NULL,
	[asiento_terminado_id] [uniqueidentifier] NULL,
	[creado_por] [uniqueidentifier] NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[numero_orden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[orden_produccion_consumo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[orden_produccion_consumo](
	[id] [uniqueidentifier] NOT NULL,
	[orden_produccion_id] [uniqueidentifier] NOT NULL,
	[producto_insumo_id] [uniqueidentifier] NOT NULL,
	[cantidad_planificada] [numeric](14, 4) NOT NULL,
	[cantidad_real] [numeric](14, 4) NOT NULL,
	[costo_unitario] [numeric](14, 4) NULL,
	[movimiento_inventario_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[plan_produccion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[plan_produccion](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[anio] [smallint] NOT NULL,
	[mes] [smallint] NULL,
	[presupuesto_id] [uniqueidentifier] NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [produccion].[plan_produccion_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [produccion].[plan_produccion_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[plan_id] [uniqueidentifier] NOT NULL,
	[producto_id] [uniqueidentifier] NOT NULL,
	[cantidad_planificada] [numeric](14, 4) NOT NULL,
	[cantidad_ejecutada] [numeric](14, 4) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[plan_id] ASC,
	[producto_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [reportes].[indicador]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [reportes].[indicador](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](40) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[categoria] [nvarchar](30) NOT NULL,
	[formula_descripcion] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [reportes].[indicador_valor]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [reportes].[indicador_valor](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[indicador_id] [int] NOT NULL,
	[periodo_id] [uniqueidentifier] NOT NULL,
	[valor] [numeric](18, 6) NOT NULL,
	[calculado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[indicador_id] ASC,
	[periodo_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [reportes].[paquete_informacion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [reportes].[paquete_informacion](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[periodo_id] [uniqueidentifier] NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[estado] [nvarchar](15) NOT NULL,
	[generado_por] [uniqueidentifier] NULL,
	[generado_en] [datetimeoffset](7) NOT NULL,
	[onat] [nvarchar](50) NULL,
	[direccion] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [reportes].[reporte_generado]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [reportes].[reporte_generado](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[nombre_reporte] [nvarchar](150) NOT NULL,
	[formato] [nvarchar](10) NOT NULL,
	[parametros_json] [nvarchar](max) NULL,
	[ruta_archivo] [nvarchar](max) NOT NULL,
	[generado_por] [uniqueidentifier] NULL,
	[generado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[cargo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[cargo](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[salario_escala_min] [numeric](12, 2) NULL,
	[salario_escala_max] [numeric](12, 2) NULL,
	[funciones] [nvarchar](max) NULL,
	[categoria_ocupacional] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[certificado_medico]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[certificado_medico](
	[id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[fecha_inicio] [date] NOT NULL,
	[fecha_fin] [date] NOT NULL,
	[dias]  AS (datediff(day,[fecha_inicio],[fecha_fin])+(1)) PERSISTED,
	[diagnostico_cie] [nvarchar](20) NULL,
	[porcentaje_subsidio] [numeric](5, 2) NOT NULL,
	[numero_certificado] [nvarchar](40) NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[concepto_nomina]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[concepto_nomina](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[tipo] [nvarchar](15) NOT NULL,
	[cuenta_contable_id] [uniqueidentifier] NULL,
	[formula] [nvarchar](max) NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[contrato_laboral]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[contrato_laboral](
	[id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[tipo_contrato] [nvarchar](20) NOT NULL,
	[fecha_inicio] [date] NOT NULL,
	[fecha_fin] [date] NULL,
	[salario_pactado] [numeric](12, 2) NOT NULL,
	[jornada_horas_semana] [numeric](4, 1) NOT NULL,
	[cargo_id] [uniqueidentifier] NOT NULL,
	[documento_url] [nvarchar](max) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[empleado]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[empleado](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[carnet_identidad] [nvarchar](11) NOT NULL,
	[nombres] [nvarchar](100) NOT NULL,
	[apellidos] [nvarchar](100) NOT NULL,
	[fecha_nacimiento] [date] NOT NULL,
	[sexo] [nvarchar](1) NULL,
	[direccion] [nvarchar](max) NULL,
	[telefono] [nvarchar](30) NULL,
	[email] [nvarchar](150) NULL,
	[cargo_id] [uniqueidentifier] NOT NULL,
	[calificacion] [nvarchar](100) NULL,
	[nivel_escolaridad] [nvarchar](50) NULL,
	[fecha_ingreso] [date] NOT NULL,
	[fecha_baja] [date] NULL,
	[motivo_baja] [nvarchar](max) NULL,
	[cuenta_bancaria_pago] [nvarchar](40) NULL,
	[estado] [nvarchar](15) NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
	[turno_trabajo_id] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[carnet_identidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[nomina_detalle]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[nomina_detalle](
	[id] [uniqueidentifier] NOT NULL,
	[periodo_nomina_id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[dias_trabajados] [numeric](4, 1) NOT NULL,
	[horas_extra] [numeric](6, 2) NOT NULL,
	[salario_devengado] [numeric](12, 2) NOT NULL,
	[total_deducciones] [numeric](12, 2) NOT NULL,
	[salario_neto] [numeric](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[periodo_nomina_id] ASC,
	[empleado_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[nomina_detalle_concepto]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[nomina_detalle_concepto](
	[id] [uniqueidentifier] NOT NULL,
	[nomina_detalle_id] [uniqueidentifier] NOT NULL,
	[concepto_id] [int] NOT NULL,
	[monto] [numeric](12, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[nomina_detalle_id] ASC,
	[concepto_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[periodo_nomina]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[periodo_nomina](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[anio] [smallint] NOT NULL,
	[mes] [smallint] NOT NULL,
	[tipo] [nvarchar](15) NOT NULL,
	[estado] [nvarchar](20) NOT NULL,
	[asiento_id] [uniqueidentifier] NULL,
	[calculado_en] [datetimeoffset](7) NULL,
	[aprobado_por] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[anio] ASC,
	[mes] ASC,
	[tipo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[plantilla_aprobada]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[plantilla_aprobada](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[sucursal_id] [uniqueidentifier] NULL,
	[cargo_id] [uniqueidentifier] NOT NULL,
	[plazas_aprobadas] [int] NOT NULL,
	[vigente_desde] [date] NOT NULL,
	[vigente_hasta] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[registro_asistencia]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[registro_asistencia](
	[id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[fecha] [date] NOT NULL,
	[hora_entrada] [time](7) NULL,
	[hora_salida] [time](7) NULL,
	[horas_extra] [numeric](4, 2) NOT NULL,
	[tipo_ausencia_id] [int] NULL,
	[observaciones] [nvarchar](max) NULL,
	[registrado_por] [uniqueidentifier] NULL,
	[turno_trabajo_id] [uniqueidentifier] NULL,
	[retardo_minutos] [int] NULL,
	[salida_temprana_minutos] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[empleado_id] ASC,
	[fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[registro_salario_tiempo_servicio]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[registro_salario_tiempo_servicio](
	[id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[anio] [smallint] NOT NULL,
	[mes] [smallint] NOT NULL,
	[dias_trabajados] [numeric](4, 1) NOT NULL,
	[salario_devengado] [numeric](12, 2) NOT NULL,
	[tiempo_servicio_acumulado_meses] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[empleado_id] ASC,
	[anio] ASC,
	[mes] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[saldo_vacaciones]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[saldo_vacaciones](
	[id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[anio] [smallint] NOT NULL,
	[dias_acumulados] [numeric](6, 2) NOT NULL,
	[dias_disfrutados] [numeric](6, 2) NOT NULL,
	[dias_compensados] [numeric](6, 2) NOT NULL,
	[saldo_actual]  AS (CONVERT([numeric](6,2),([dias_acumulados]-[dias_disfrutados])-[dias_compensados])) PERSISTED,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[empleado_id] ASC,
	[anio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[tipo_ausencia]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[tipo_ausencia](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](30) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[remunerada] [bit] NOT NULL,
	[afecta_vacaciones] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[turno_trabajo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[turno_trabajo](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[codigo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[hora_entrada] [time](7) NOT NULL,
	[hora_salida] [time](7) NOT NULL,
	[tolerancia_minutos] [int] NOT NULL,
	[es_nocturno] [bit] NOT NULL,
	[activo] [bit] NOT NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_turno_trabajo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_turno_trabajo_entidad_codigo] UNIQUE NONCLUSTERED 
(
	[entidad_id] ASC,
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [rrhh].[utile_responsabilidad]    Script Date: 12/10/2026 11:50:31 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [rrhh].[utile_responsabilidad](
	[id] [uniqueidentifier] NOT NULL,
	[entidad_id] [uniqueidentifier] NOT NULL,
	[empleado_id] [uniqueidentifier] NOT NULL,
	[descripcion] [nvarchar](200) NOT NULL,
	[numero_serie] [nvarchar](50) NULL,
	[fecha_entrega] [date] NOT NULL,
	[fecha_devolucion] [date] NULL,
	[estado_entrega] [nvarchar](50) NULL,
	[observaciones] [nvarchar](max) NULL,
	[creado_en] [datetimeoffset](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [uq_cliente_nit_o_ci]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_cliente_nit_o_ci] ON [comercial].[cliente]
(
	[entidad_id] ASC,
	[nit_o_ci] ASC
)
WHERE ([nit_o_ci] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_contrato_numero_tercero]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ_contrato_numero_tercero] ON [comercial].[contrato_economico]
(
	[entidad_id] ASC,
	[numero_contrato] ASC,
	[cliente_id] ASC,
	[proveedor_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_suplemento_contrato_vigente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_suplemento_contrato_vigente] ON [comercial].[contrato_economico_suplemento]
(
	[contrato_id] ASC,
	[estado] ASC,
	[fecha_fin] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_factura_canal]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_factura_canal] ON [comercial].[factura_venta]
(
	[canal_venta] ASC,
	[fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_factura_cliente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_factura_cliente] ON [comercial].[factura_venta]
(
	[cliente_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_factura_pos_sesion]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_factura_pos_sesion] ON [comercial].[factura_venta]
(
	[sesion_caja_pos_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [uq_proveedor_nit]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_proveedor_nit] ON [comercial].[proveedor]
(
	[entidad_id] ASC,
	[nit] ASC
)
WHERE ([nit] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_asiento_origen]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_asiento_origen] ON [contabilidad].[asiento_contable]
(
	[modulo_origen] ASC,
	[documento_origen_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_asiento_periodo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_asiento_periodo] ON [contabilidad].[asiento_contable]
(
	[periodo_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_asiento_detalle_cuenta]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_asiento_detalle_cuenta] ON [contabilidad].[asiento_detalle]
(
	[cuenta_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_asiento_detalle_tercero]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_asiento_detalle_tercero] ON [contabilidad].[asiento_detalle]
(
	[tercero_tipo] ASC,
	[tercero_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_cuenta_padre]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_cuenta_padre] ON [contabilidad].[cuenta_contable]
(
	[cuenta_padre_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_cxc_cliente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_cxc_cliente] ON [contabilidad].[cuenta_por_cobrar]
(
	[cliente_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_cxc_vencimiento]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_cxc_vencimiento] ON [contabilidad].[cuenta_por_cobrar]
(
	[fecha_vencimiento] ASC
)
WHERE ([estado] IN ('PENDIENTE', 'PARCIAL'))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_cxp_proveedor]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_cxp_proveedor] ON [contabilidad].[cuenta_por_pagar]
(
	[proveedor_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_cxp_vencimiento]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_cxp_vencimiento] ON [contabilidad].[cuenta_por_pagar]
(
	[fecha_vencimiento] ASC
)
WHERE ([estado] IN ('PENDIENTE', 'PARCIAL'))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_api_log_cliente_fecha]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_api_log_cliente_fecha] ON [integracion].[api_log]
(
	[api_cliente_id] ASC,
	[ocurrido_en] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_api_token_cliente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_api_token_cliente] ON [integracion].[api_token]
(
	[api_cliente_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_pos_sync_dispositivo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_pos_sync_dispositivo] ON [integracion].[pos_sync_log]
(
	[dispositivo_pos_id] ASC,
	[iniciado_en] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_pos_venta_pendiente_estado]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_pos_venta_pendiente_estado] ON [integracion].[pos_venta_pendiente]
(
	[estado] ASC
)
WHERE ([estado]='PENDIENTE')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_sesion_caja_caja]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_sesion_caja_caja] ON [integracion].[sesion_caja_pos]
(
	[caja_id] ASC,
	[estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_sesion_caja_dispositivo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_sesion_caja_dispositivo] ON [integracion].[sesion_caja_pos]
(
	[dispositivo_pos_id] ASC,
	[estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [uq_sesion_caja_abierta]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_sesion_caja_abierta] ON [integracion].[sesion_caja_pos]
(
	[caja_id] ASC
)
WHERE ([estado]='ABIERTA')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_webhook_entrega_pendiente]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_webhook_entrega_pendiente] ON [integracion].[webhook_entrega]
(
	[proximo_reintento_en] ASC
)
WHERE ([exitoso]=(0))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_existencia_bajo_minimo]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_existencia_bajo_minimo] ON [inventario].[existencia]
(
	[almacen_id] ASC,
	[cantidad] ASC,
	[stock_minimo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_mov_inv_fecha]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_mov_inv_fecha] ON [inventario].[movimiento_inventario]
(
	[fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_mov_inv_referencia]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_mov_inv_referencia] ON [inventario].[movimiento_inventario]
(
	[referencia_externa_tipo] ASC,
	[referencia_externa_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_mov_inv_det_producto]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_mov_inv_det_producto] ON [inventario].[movimiento_inventario_detalle]
(
	[producto_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_producto_codigo_barras]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_producto_codigo_barras] ON [inventario].[producto]
(
	[codigo_barras] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_producto_nombre]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_producto_nombre] ON [inventario].[producto]
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [uq_producto_codigo_barras]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_producto_codigo_barras] ON [inventario].[producto]
(
	[entidad_id] ASC,
	[codigo_barras] ASC
)
WHERE ([codigo_barras] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_auditoria_fecha]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_auditoria_fecha] ON [nucleo].[auditoria]
(
	[ocurrido_en] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_auditoria_tabla_registro]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_auditoria_tabla_registro] ON [nucleo].[auditoria]
(
	[esquema_tabla] ASC,
	[registro_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_auditoria_usuario]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_auditoria_usuario] ON [nucleo].[auditoria]
(
	[usuario_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_feedback_entidad]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_feedback_entidad] ON [nucleo].[feedback]
(
	[entidad_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_feedback_fecha]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [idx_feedback_fecha] ON [nucleo].[feedback]
(
	[creado_en] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_notificacion_bandeja]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_notificacion_bandeja] ON [nucleo].[notificacion]
(
	[entidad_id] ASC,
	[leida] ASC,
	[creado_en] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_notificacion_usuario]    Script Date: 12/10/2026 11:50:31 a. m. ******/
CREATE NONCLUSTERED INDEX [IX_notificacion_usuario] ON [nucleo].[notificacion]
(
	[usuario_id] ASC,
	[leida] ASC,
	[creado_en] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [comercial].[cliente] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[cliente] ADD  DEFAULT ('JURIDICA') FOR [tipo_persona]
GO
ALTER TABLE [comercial].[cliente] ADD  DEFAULT ('MINORISTA') FOR [segmento]
GO
ALTER TABLE [comercial].[cliente] ADD  DEFAULT ((0)) FOR [limite_credito]
GO
ALTER TABLE [comercial].[cliente] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [comercial].[cliente] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [comercial].[contrato_economico] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[contrato_economico] ADD  DEFAULT ('VIGENTE') FOR [estado]
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] ADD  CONSTRAINT [DF_suplemento_estado]  DEFAULT ('VIGENTE') FOR [estado]
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] ADD  CONSTRAINT [DF_suplemento_creado]  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [comercial].[devolucion_venta] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[devolucion_venta] ADD  DEFAULT (CONVERT([date],sysdatetimeoffset())) FOR [fecha]
GO
ALTER TABLE [comercial].[devolucion_venta_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ('A') FOR [serie]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ('ERP') FOR [canal_venta]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ('MINORISTA') FOR [tipo_venta]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT (sysdatetimeoffset()) FOR [fecha]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ((0)) FOR [subtotal]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ((0)) FOR [descuento_total]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ((0)) FOR [impuesto_ventas_total]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ((0)) FOR [total]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ('CUP') FOR [moneda]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT ('EMITIDA') FOR [estado]
GO
ALTER TABLE [comercial].[factura_venta] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [comercial].[factura_venta_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[factura_venta_detalle] ADD  DEFAULT ((0)) FOR [descuento_porcentaje]
GO
ALTER TABLE [comercial].[factura_venta_detalle] ADD  DEFAULT ((0)) FOR [impuesto_porcentaje]
GO
ALTER TABLE [comercial].[forma_pago_venta] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[forma_pago_venta] ADD  DEFAULT ((0)) FOR [vuelto_entregado]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT (CONVERT([date],sysdatetimeoffset())) FOR [fecha]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT ('CUP') FOR [moneda]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT ((0)) FOR [subtotal]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT ((0)) FOR [total]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT ('BORRADOR') FOR [estado]
GO
ALTER TABLE [comercial].[orden_compra] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [comercial].[orden_compra_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[orden_compra_detalle] ADD  DEFAULT ((0)) FOR [cantidad_recibida]
GO
ALTER TABLE [comercial].[proveedor] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[proveedor] ADD  DEFAULT ('JURIDICA') FOR [tipo_persona]
GO
ALTER TABLE [comercial].[proveedor] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [comercial].[proveedor] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [comercial].[recepcion_compra] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [comercial].[recepcion_compra] ADD  DEFAULT (CONVERT([date],sysdatetimeoffset())) FOR [fecha]
GO
ALTER TABLE [comercial].[tope_precio_mfp] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[activo_fijo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[activo_fijo] ADD  DEFAULT ((0)) FOR [valor_residual]
GO
ALTER TABLE [contabilidad].[activo_fijo] ADD  DEFAULT ('LINEA_RECTA') FOR [metodo_depreciacion]
GO
ALTER TABLE [contabilidad].[activo_fijo] ADD  DEFAULT ((0)) FOR [depreciacion_acumulada]
GO
ALTER TABLE [contabilidad].[activo_fijo] ADD  DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [contabilidad].[activo_fijo_depreciacion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[activo_fijo_depreciacion] ADD  DEFAULT (sysdatetimeoffset()) FOR [calculado_en]
GO
ALTER TABLE [contabilidad].[asiento_contable] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[asiento_contable] ADD  DEFAULT ('CONTABILIDAD') FOR [modulo_origen]
GO
ALTER TABLE [contabilidad].[asiento_contable] ADD  DEFAULT ('CONTABILIZADO') FOR [estado]
GO
ALTER TABLE [contabilidad].[asiento_contable] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [contabilidad].[asiento_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[asiento_detalle] ADD  DEFAULT ((0)) FOR [debe]
GO
ALTER TABLE [contabilidad].[asiento_detalle] ADD  DEFAULT ((0)) FOR [haber]
GO
ALTER TABLE [contabilidad].[caja] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[caja] ADD  DEFAULT ((0)) FOR [saldo_actual]
GO
ALTER TABLE [contabilidad].[caja] ADD  DEFAULT ((1)) FOR [activa]
GO
ALTER TABLE [contabilidad].[centro_costo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[centro_costo] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [contabilidad].[cuenta_bancaria] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[cuenta_bancaria] ADD  DEFAULT ((0)) FOR [saldo_actual]
GO
ALTER TABLE [contabilidad].[cuenta_bancaria] ADD  DEFAULT ((1)) FOR [activa]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT ((1)) FOR [nivel]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT ((1)) FOR [acepta_movimiento]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT ((0)) FOR [requiere_centro_costo]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT ((0)) FOR [requiere_tercero]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT ('CUP') FOR [moneda]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [contabilidad].[cuenta_contable] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar] ADD  DEFAULT ('CUP') FOR [moneda]
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar] ADD  DEFAULT ('PENDIENTE') FOR [estado]
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar] ADD  DEFAULT ('CUP') FOR [moneda]
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar] ADD  DEFAULT ('PENDIENTE') FOR [estado]
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [contabilidad].[declaracion_jurada] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[declaracion_jurada] ADD  DEFAULT ((0)) FOR [monto_pagado]
GO
ALTER TABLE [contabilidad].[declaracion_jurada] ADD  DEFAULT ('PENDIENTE') FOR [estado]
GO
ALTER TABLE [contabilidad].[declaracion_jurada] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [contabilidad].[movimiento_bancario] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[movimiento_bancario] ADD  DEFAULT ((0)) FOR [conciliado]
GO
ALTER TABLE [contabilidad].[pago_aplicado] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[periodo_contable] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[periodo_contable] ADD  DEFAULT ('ABIERTO') FOR [estado]
GO
ALTER TABLE [contabilidad].[presupuesto] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[presupuesto] ADD  DEFAULT ('BORRADOR') FOR [estado]
GO
ALTER TABLE [contabilidad].[presupuesto] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [contabilidad].[presupuesto_linea] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [contabilidad].[presupuesto_linea] ADD  DEFAULT ((0)) FOR [monto_planificado]
GO
ALTER TABLE [integracion].[api_cliente] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[api_cliente] ADD  DEFAULT ('POS') FOR [tipo]
GO
ALTER TABLE [integracion].[api_cliente] ADD  DEFAULT (N'[]') FOR [scopes]
GO
ALTER TABLE [integracion].[api_cliente] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [integracion].[api_cliente] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [integracion].[api_log] ADD  DEFAULT (sysdatetimeoffset()) FOR [ocurrido_en]
GO
ALTER TABLE [integracion].[api_rate_limit] ADD  DEFAULT ((120)) FOR [solicitudes_por_minuto]
GO
ALTER TABLE [integracion].[api_rate_limit] ADD  DEFAULT (sysdatetimeoffset()) FOR [actualizado_en]
GO
ALTER TABLE [integracion].[api_token] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[api_token] ADD  DEFAULT ('ACCESS') FOR [tipo]
GO
ALTER TABLE [integracion].[api_token] ADD  DEFAULT (sysdatetimeoffset()) FOR [emitido_en]
GO
ALTER TABLE [integracion].[api_token] ADD  DEFAULT ((0)) FOR [revocado]
GO
ALTER TABLE [integracion].[dispositivo_pos] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[dispositivo_pos] ADD  DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [integracion].[dispositivo_pos] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [integracion].[movimiento_caja_pos] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[movimiento_caja_pos] ADD  DEFAULT (sysdatetimeoffset()) FOR [ocurrido_en]
GO
ALTER TABLE [integracion].[pos_rango_numeracion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[pos_rango_numeracion] ADD  DEFAULT ('FACTURA_VENTA') FOR [tipo_documento]
GO
ALTER TABLE [integracion].[pos_rango_numeracion] ADD  DEFAULT (sysdatetimeoffset()) FOR [asignado_en]
GO
ALTER TABLE [integracion].[pos_rango_numeracion] ADD  DEFAULT ((0)) FOR [agotado]
GO
ALTER TABLE [integracion].[pos_sync_log] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[pos_sync_log] ADD  DEFAULT ((0)) FOR [registros_procesados]
GO
ALTER TABLE [integracion].[pos_sync_log] ADD  DEFAULT ('EN_PROGRESO') FOR [estado]
GO
ALTER TABLE [integracion].[pos_sync_log] ADD  DEFAULT (sysdatetimeoffset()) FOR [iniciado_en]
GO
ALTER TABLE [integracion].[pos_venta_pendiente] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[pos_venta_pendiente] ADD  DEFAULT (sysdatetimeoffset()) FOR [fecha_recibido_servidor]
GO
ALTER TABLE [integracion].[pos_venta_pendiente] ADD  DEFAULT ('PENDIENTE') FOR [estado]
GO
ALTER TABLE [integracion].[pos_venta_pendiente] ADD  DEFAULT ((0)) FOR [intentos_procesamiento]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT (sysdatetimeoffset()) FOR [fecha_apertura]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [monto_apertura]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [total_ventas]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [total_efectivo]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [total_transfermovil]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [total_enzona]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [total_otros_medios]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ((0)) FOR [cantidad_facturas]
GO
ALTER TABLE [integracion].[sesion_caja_pos] ADD  DEFAULT ('ABIERTA') FOR [estado]
GO
ALTER TABLE [integracion].[webhook_entrega] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[webhook_entrega] ADD  DEFAULT ((1)) FOR [intento_numero]
GO
ALTER TABLE [integracion].[webhook_entrega] ADD  DEFAULT ((0)) FOR [exitoso]
GO
ALTER TABLE [integracion].[webhook_entrega] ADD  DEFAULT (sysdatetimeoffset()) FOR [enviado_en]
GO
ALTER TABLE [integracion].[webhook_suscripcion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [integracion].[webhook_suscripcion] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [integracion].[webhook_suscripcion] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [inventario].[almacen] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[almacen] ADD  DEFAULT ((0)) FOR [es_punto_venta]
GO
ALTER TABLE [inventario].[almacen] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [inventario].[conteo_fisico] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[conteo_fisico] ADD  DEFAULT ('PARCIAL') FOR [tipo]
GO
ALTER TABLE [inventario].[conteo_fisico] ADD  DEFAULT ('EN_PROCESO') FOR [estado]
GO
ALTER TABLE [inventario].[conteo_fisico] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [inventario].[conteo_fisico_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[existencia] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[existencia] ADD  DEFAULT ((0)) FOR [cantidad]
GO
ALTER TABLE [inventario].[existencia] ADD  DEFAULT ((0)) FOR [costo_promedio]
GO
ALTER TABLE [inventario].[existencia] ADD  DEFAULT ((0)) FOR [stock_minimo]
GO
ALTER TABLE [inventario].[existencia] ADD  DEFAULT (sysdatetimeoffset()) FOR [actualizado_en]
GO
ALTER TABLE [inventario].[familia_producto] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[lista_precio] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[lista_precio] ADD  DEFAULT ('GENERAL') FOR [canal]
GO
ALTER TABLE [inventario].[lista_precio] ADD  DEFAULT ((1)) FOR [activa]
GO
ALTER TABLE [inventario].[lista_precio_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[movimiento_inventario] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[movimiento_inventario] ADD  DEFAULT (sysdatetimeoffset()) FOR [fecha]
GO
ALTER TABLE [inventario].[movimiento_inventario] ADD  DEFAULT ('ERP') FOR [canal]
GO
ALTER TABLE [inventario].[movimiento_inventario] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [inventario].[movimiento_inventario_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[producto] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [inventario].[producto] ADD  DEFAULT ('TERMINADO') FOR [tipo]
GO
ALTER TABLE [inventario].[producto] ADD  DEFAULT ((1)) FOR [aplica_impuesto_ventas]
GO
ALTER TABLE [inventario].[producto] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [inventario].[producto] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [inventario].[producto] ADD  DEFAULT (sysdatetimeoffset()) FOR [actualizado_en]
GO
ALTER TABLE [inventario].[tipo_movimiento] ADD  DEFAULT ((1)) FOR [afecta_costo]
GO
ALTER TABLE [inventario].[unidad_medida] ADD  DEFAULT ((0)) FOR [es_fraccionable]
GO
ALTER TABLE [nucleo].[auditoria] ADD  DEFAULT ('ERP') FOR [canal]
GO
ALTER TABLE [nucleo].[auditoria] ADD  DEFAULT (sysdatetimeoffset()) FOR [ocurrido_en]
GO
ALTER TABLE [nucleo].[backup_log] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [nucleo].[backup_log] ADD  DEFAULT ('EN_PROGRESO') FOR [estado]
GO
ALTER TABLE [nucleo].[backup_log] ADD  DEFAULT (sysdatetimeoffset()) FOR [iniciado_en]
GO
ALTER TABLE [nucleo].[consecutivo] ADD  DEFAULT ('A') FOR [serie]
GO
ALTER TABLE [nucleo].[consecutivo] ADD  DEFAULT ((0)) FOR [ultimo_numero]
GO
ALTER TABLE [nucleo].[consecutivo] ADD  DEFAULT ((8)) FOR [longitud_padding]
GO
ALTER TABLE [nucleo].[consecutivo] ADD  DEFAULT (sysdatetimeoffset()) FOR [actualizado_en]
GO
ALTER TABLE [nucleo].[entidad] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [nucleo].[entidad] ADD  DEFAULT ('S.U.R.L.') FOR [forma_juridica]
GO
ALTER TABLE [nucleo].[entidad] ADD  DEFAULT ('CUP') FOR [moneda_base]
GO
ALTER TABLE [nucleo].[entidad] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [nucleo].[entidad] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [nucleo].[entidad] ADD  DEFAULT (sysdatetimeoffset()) FOR [actualizado_en]
GO
ALTER TABLE [nucleo].[feedback] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [nucleo].[feedback] ADD  DEFAULT ('PENDIENTE') FOR [estado]
GO
ALTER TABLE [nucleo].[feedback] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [nucleo].[notificacion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [nucleo].[notificacion] ADD  DEFAULT (N'info') FOR [tipo]
GO
ALTER TABLE [nucleo].[notificacion] ADD  DEFAULT ((0)) FOR [leida]
GO
ALTER TABLE [nucleo].[notificacion] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [nucleo].[parametro_sistema] ADD  DEFAULT ('STRING') FOR [tipo_dato]
GO
ALTER TABLE [nucleo].[parametro_sistema] ADD  DEFAULT (CONVERT([date],sysdatetimeoffset())) FOR [vigente_desde]
GO
ALTER TABLE [nucleo].[rol] ADD  DEFAULT ((0)) FOR [es_sistema]
GO
ALTER TABLE [nucleo].[rol] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [nucleo].[sucursal] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [nucleo].[sucursal] ADD  DEFAULT ('ALMACEN') FOR [tipo]
GO
ALTER TABLE [nucleo].[sucursal] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [nucleo].[sucursal] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [nucleo].[usuario] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [nucleo].[usuario] ADD  DEFAULT ((1)) FOR [debe_cambiar_pass]
GO
ALTER TABLE [nucleo].[usuario] ADD  DEFAULT ((0)) FOR [intentos_fallidos]
GO
ALTER TABLE [nucleo].[usuario] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [nucleo].[usuario] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [nucleo].[usuario] ADD  DEFAULT (sysdatetimeoffset()) FOR [actualizado_en]
GO
ALTER TABLE [nucleo].[usuario_rol] ADD  DEFAULT (sysdatetimeoffset()) FOR [asignado_en]
GO
ALTER TABLE [produccion].[analisis_desviacion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[analisis_desviacion] ADD  DEFAULT (sysdatetimeoffset()) FOR [analizado_en]
GO
ALTER TABLE [produccion].[equipo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[equipo] ADD  DEFAULT ('OPERATIVO') FOR [estado]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT ((1)) FOR [version]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT ((0)) FOR [costo_materia_prima]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT ((0)) FOR [costo_mano_obra]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT ((0)) FOR [gastos_indirectos]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT ('VIGENTE') FOR [estado]
GO
ALTER TABLE [produccion].[ficha_costo] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [produccion].[lista_materiales] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[lista_materiales] ADD  DEFAULT ((1)) FOR [version]
GO
ALTER TABLE [produccion].[lista_materiales] ADD  DEFAULT ((1)) FOR [activa]
GO
ALTER TABLE [produccion].[lista_materiales] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [produccion].[lista_materiales_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[lista_materiales_detalle] ADD  DEFAULT ((0)) FOR [porcentaje_merma]
GO
ALTER TABLE [produccion].[mantenimiento_programado] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[mantenimiento_programado] ADD  DEFAULT ('PREVENTIVO') FOR [tipo]
GO
ALTER TABLE [produccion].[mantenimiento_programado] ADD  DEFAULT ('PROGRAMADO') FOR [estado]
GO
ALTER TABLE [produccion].[merma] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[merma] ADD  DEFAULT (CONVERT([date],sysdatetimeoffset())) FOR [fecha]
GO
ALTER TABLE [produccion].[orden_produccion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[orden_produccion] ADD  DEFAULT ((0)) FOR [cantidad_producida]
GO
ALTER TABLE [produccion].[orden_produccion] ADD  DEFAULT ('PLANIFICADA') FOR [estado]
GO
ALTER TABLE [produccion].[orden_produccion] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [produccion].[orden_produccion_consumo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[orden_produccion_consumo] ADD  DEFAULT ((0)) FOR [cantidad_real]
GO
ALTER TABLE [produccion].[plan_produccion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[plan_produccion] ADD  DEFAULT ('BORRADOR') FOR [estado]
GO
ALTER TABLE [produccion].[plan_produccion] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [produccion].[plan_produccion_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [produccion].[plan_produccion_detalle] ADD  DEFAULT ((0)) FOR [cantidad_ejecutada]
GO
ALTER TABLE [reportes].[indicador_valor] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [reportes].[indicador_valor] ADD  DEFAULT (sysdatetimeoffset()) FOR [calculado_en]
GO
ALTER TABLE [reportes].[paquete_informacion] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [reportes].[paquete_informacion] ADD  DEFAULT ('GENERADO') FOR [estado]
GO
ALTER TABLE [reportes].[paquete_informacion] ADD  DEFAULT (sysdatetimeoffset()) FOR [generado_en]
GO
ALTER TABLE [reportes].[reporte_generado] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [reportes].[reporte_generado] ADD  DEFAULT (sysdatetimeoffset()) FOR [generado_en]
GO
ALTER TABLE [rrhh].[cargo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[certificado_medico] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[certificado_medico] ADD  DEFAULT ((100)) FOR [porcentaje_subsidio]
GO
ALTER TABLE [rrhh].[certificado_medico] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [rrhh].[contrato_laboral] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[contrato_laboral] ADD  DEFAULT ((44)) FOR [jornada_horas_semana]
GO
ALTER TABLE [rrhh].[contrato_laboral] ADD  DEFAULT ('VIGENTE') FOR [estado]
GO
ALTER TABLE [rrhh].[contrato_laboral] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [rrhh].[empleado] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[empleado] ADD  DEFAULT ('ACTIVO') FOR [estado]
GO
ALTER TABLE [rrhh].[empleado] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [rrhh].[nomina_detalle] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[nomina_detalle] ADD  DEFAULT ((0)) FOR [dias_trabajados]
GO
ALTER TABLE [rrhh].[nomina_detalle] ADD  DEFAULT ((0)) FOR [horas_extra]
GO
ALTER TABLE [rrhh].[nomina_detalle] ADD  DEFAULT ((0)) FOR [salario_devengado]
GO
ALTER TABLE [rrhh].[nomina_detalle] ADD  DEFAULT ((0)) FOR [total_deducciones]
GO
ALTER TABLE [rrhh].[nomina_detalle] ADD  DEFAULT ((0)) FOR [salario_neto]
GO
ALTER TABLE [rrhh].[nomina_detalle_concepto] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[periodo_nomina] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[periodo_nomina] ADD  DEFAULT ('MENSUAL') FOR [tipo]
GO
ALTER TABLE [rrhh].[periodo_nomina] ADD  DEFAULT ('PRENOMINA') FOR [estado]
GO
ALTER TABLE [rrhh].[plantilla_aprobada] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[plantilla_aprobada] ADD  DEFAULT ((1)) FOR [plazas_aprobadas]
GO
ALTER TABLE [rrhh].[registro_asistencia] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[registro_asistencia] ADD  DEFAULT ((0)) FOR [horas_extra]
GO
ALTER TABLE [rrhh].[registro_salario_tiempo_servicio] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[saldo_vacaciones] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[saldo_vacaciones] ADD  DEFAULT ((0)) FOR [dias_acumulados]
GO
ALTER TABLE [rrhh].[saldo_vacaciones] ADD  DEFAULT ((0)) FOR [dias_disfrutados]
GO
ALTER TABLE [rrhh].[saldo_vacaciones] ADD  DEFAULT ((0)) FOR [dias_compensados]
GO
ALTER TABLE [rrhh].[tipo_ausencia] ADD  DEFAULT ((1)) FOR [remunerada]
GO
ALTER TABLE [rrhh].[tipo_ausencia] ADD  DEFAULT ((0)) FOR [afecta_vacaciones]
GO
ALTER TABLE [rrhh].[turno_trabajo] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[turno_trabajo] ADD  CONSTRAINT [DF_turno_tolerancia]  DEFAULT ((15)) FOR [tolerancia_minutos]
GO
ALTER TABLE [rrhh].[turno_trabajo] ADD  CONSTRAINT [DF_turno_nocturno]  DEFAULT ((0)) FOR [es_nocturno]
GO
ALTER TABLE [rrhh].[turno_trabajo] ADD  CONSTRAINT [DF_turno_activo]  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [rrhh].[turno_trabajo] ADD  CONSTRAINT [DF_turno_creado]  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [rrhh].[utile_responsabilidad] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [rrhh].[utile_responsabilidad] ADD  DEFAULT (CONVERT([date],getdate())) FOR [fecha_entrega]
GO
ALTER TABLE [rrhh].[utile_responsabilidad] ADD  DEFAULT (sysdatetimeoffset()) FOR [creado_en]
GO
ALTER TABLE [comercial].[cliente]  WITH CHECK ADD FOREIGN KEY([cuenta_contable_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [comercial].[cliente]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[cliente]  WITH CHECK ADD FOREIGN KEY([lista_precio_id])
REFERENCES [inventario].[lista_precio] ([id])
GO
ALTER TABLE [comercial].[contrato_economico]  WITH CHECK ADD FOREIGN KEY([cliente_id])
REFERENCES [comercial].[cliente] ([id])
GO
ALTER TABLE [comercial].[contrato_economico]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[contrato_economico]  WITH CHECK ADD FOREIGN KEY([proveedor_id])
REFERENCES [comercial].[proveedor] ([id])
GO
ALTER TABLE [comercial].[contrato_economico_suplemento]  WITH CHECK ADD  CONSTRAINT [FK_suplemento_contrato] FOREIGN KEY([contrato_id])
REFERENCES [comercial].[contrato_economico] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] CHECK CONSTRAINT [FK_suplemento_contrato]
GO
ALTER TABLE [comercial].[contrato_economico_suplemento]  WITH CHECK ADD  CONSTRAINT [FK_suplemento_entidad] FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] CHECK CONSTRAINT [FK_suplemento_entidad]
GO
ALTER TABLE [comercial].[devolucion_venta]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [comercial].[devolucion_venta]  WITH CHECK ADD FOREIGN KEY([autorizado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [comercial].[devolucion_venta]  WITH CHECK ADD FOREIGN KEY([factura_id])
REFERENCES [comercial].[factura_venta] ([id])
GO
ALTER TABLE [comercial].[devolucion_venta]  WITH CHECK ADD FOREIGN KEY([movimiento_inventario_id])
REFERENCES [inventario].[movimiento_inventario] ([id])
GO
ALTER TABLE [comercial].[devolucion_venta_detalle]  WITH CHECK ADD FOREIGN KEY([devolucion_id])
REFERENCES [comercial].[devolucion_venta] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [comercial].[devolucion_venta_detalle]  WITH CHECK ADD FOREIGN KEY([factura_detalle_id])
REFERENCES [comercial].[factura_venta_detalle] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([almacen_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([cliente_id])
REFERENCES [comercial].[cliente] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([contrato_id])
REFERENCES [comercial].[contrato_economico] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([cuenta_por_cobrar_id])
REFERENCES [contabilidad].[cuenta_por_cobrar] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD  CONSTRAINT [fk_factura_dispositivo] FOREIGN KEY([dispositivo_pos_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [comercial].[factura_venta] CHECK CONSTRAINT [fk_factura_dispositivo]
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD  CONSTRAINT [fk_factura_sesion_caja] FOREIGN KEY([sesion_caja_pos_id])
REFERENCES [integracion].[sesion_caja_pos] ([id])
GO
ALTER TABLE [comercial].[factura_venta] CHECK CONSTRAINT [fk_factura_sesion_caja]
GO
ALTER TABLE [comercial].[factura_venta_detalle]  WITH CHECK ADD FOREIGN KEY([factura_id])
REFERENCES [comercial].[factura_venta] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [comercial].[factura_venta_detalle]  WITH CHECK ADD FOREIGN KEY([movimiento_inventario_id])
REFERENCES [inventario].[movimiento_inventario] ([id])
GO
ALTER TABLE [comercial].[factura_venta_detalle]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [comercial].[forma_pago_venta]  WITH CHECK ADD FOREIGN KEY([factura_id])
REFERENCES [comercial].[factura_venta] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD FOREIGN KEY([almacen_destino_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD FOREIGN KEY([aprobado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD FOREIGN KEY([contrato_id])
REFERENCES [comercial].[contrato_economico] ([id])
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD FOREIGN KEY([proveedor_id])
REFERENCES [comercial].[proveedor] ([id])
GO
ALTER TABLE [comercial].[orden_compra_detalle]  WITH CHECK ADD FOREIGN KEY([orden_compra_id])
REFERENCES [comercial].[orden_compra] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [comercial].[orden_compra_detalle]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [comercial].[proveedor]  WITH CHECK ADD FOREIGN KEY([cuenta_contable_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [comercial].[proveedor]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[recepcion_compra]  WITH CHECK ADD FOREIGN KEY([cuenta_por_pagar_id])
REFERENCES [contabilidad].[cuenta_por_pagar] ([id])
GO
ALTER TABLE [comercial].[recepcion_compra]  WITH CHECK ADD FOREIGN KEY([movimiento_inventario_id])
REFERENCES [inventario].[movimiento_inventario] ([id])
GO
ALTER TABLE [comercial].[recepcion_compra]  WITH CHECK ADD FOREIGN KEY([orden_compra_id])
REFERENCES [comercial].[orden_compra] ([id])
GO
ALTER TABLE [comercial].[recepcion_compra]  WITH CHECK ADD FOREIGN KEY([recibido_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [comercial].[tope_precio_mfp]  WITH CHECK ADD FOREIGN KEY([familia_id])
REFERENCES [inventario].[familia_producto] ([id])
GO
ALTER TABLE [comercial].[tope_precio_mfp]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD FOREIGN KEY([cuenta_activo_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD FOREIGN KEY([cuenta_depreciacion_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD FOREIGN KEY([cuenta_gasto_dep_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo_depreciacion]  WITH CHECK ADD FOREIGN KEY([activo_fijo_id])
REFERENCES [contabilidad].[activo_fijo] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo_depreciacion]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[activo_fijo_depreciacion]  WITH CHECK ADD FOREIGN KEY([periodo_id])
REFERENCES [contabilidad].[periodo_contable] ([id])
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD FOREIGN KEY([asiento_reversion_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD FOREIGN KEY([periodo_id])
REFERENCES [contabilidad].[periodo_contable] ([id])
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD FOREIGN KEY([tipo_comprobante_id])
REFERENCES [contabilidad].[tipo_comprobante] ([id])
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH CHECK ADD FOREIGN KEY([centro_costo_id])
REFERENCES [contabilidad].[centro_costo] ([id])
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH CHECK ADD FOREIGN KEY([cuenta_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[caja]  WITH CHECK ADD FOREIGN KEY([cuenta_contable_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[caja]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[caja]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [contabilidad].[centro_costo]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[centro_costo]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_bancaria]  WITH CHECK ADD FOREIGN KEY([cuenta_contable_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_bancaria]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_contable]  WITH CHECK ADD FOREIGN KEY([cuenta_padre_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_contable]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar]  WITH CHECK ADD  CONSTRAINT [FK__cuenta_po__asien__59785AS3] FOREIGN KEY([cliente_id])
REFERENCES [comercial].[cliente] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar] CHECK CONSTRAINT [FK__cuenta_po__asien__59785AS3]
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar]  WITH CHECK ADD FOREIGN KEY([asiento_origen_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar]  WITH CHECK ADD FOREIGN KEY([asiento_origen_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar]  WITH CHECK ADD  CONSTRAINT [FK__cuenta_po__asien__625F763W] FOREIGN KEY([proveedor_id])
REFERENCES [comercial].[proveedor] ([id])
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar] CHECK CONSTRAINT [FK__cuenta_po__asien__625F763W]
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[declaracion_jurada]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[declaracion_jurada]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[declaracion_jurada]  WITH CHECK ADD FOREIGN KEY([generado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [contabilidad].[declaracion_jurada]  WITH CHECK ADD FOREIGN KEY([periodo_id])
REFERENCES [contabilidad].[periodo_contable] ([id])
GO
ALTER TABLE [contabilidad].[declaracion_jurada]  WITH CHECK ADD FOREIGN KEY([tipo_obligacion_id])
REFERENCES [contabilidad].[tipo_obligacion_fiscal] ([id])
GO
ALTER TABLE [contabilidad].[movimiento_bancario]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[movimiento_bancario]  WITH CHECK ADD FOREIGN KEY([cuenta_bancaria_id])
REFERENCES [contabilidad].[cuenta_bancaria] ([id])
GO
ALTER TABLE [contabilidad].[pago_aplicado]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [contabilidad].[pago_aplicado]  WITH CHECK ADD FOREIGN KEY([cuenta_por_cobrar_id])
REFERENCES [contabilidad].[cuenta_por_cobrar] ([id])
GO
ALTER TABLE [contabilidad].[pago_aplicado]  WITH CHECK ADD FOREIGN KEY([cuenta_por_pagar_id])
REFERENCES [contabilidad].[cuenta_por_pagar] ([id])
GO
ALTER TABLE [contabilidad].[periodo_contable]  WITH CHECK ADD FOREIGN KEY([cerrado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [contabilidad].[periodo_contable]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[presupuesto]  WITH CHECK ADD FOREIGN KEY([aprobado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [contabilidad].[presupuesto]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [contabilidad].[presupuesto_linea]  WITH CHECK ADD FOREIGN KEY([centro_costo_id])
REFERENCES [contabilidad].[centro_costo] ([id])
GO
ALTER TABLE [contabilidad].[presupuesto_linea]  WITH CHECK ADD FOREIGN KEY([cuenta_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [contabilidad].[presupuesto_linea]  WITH CHECK ADD FOREIGN KEY([presupuesto_id])
REFERENCES [contabilidad].[presupuesto] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [integracion].[api_cliente]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [integracion].[api_cliente]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [integracion].[api_log]  WITH CHECK ADD FOREIGN KEY([api_cliente_id])
REFERENCES [integracion].[api_cliente] ([id])
GO
ALTER TABLE [integracion].[api_rate_limit]  WITH CHECK ADD FOREIGN KEY([api_cliente_id])
REFERENCES [integracion].[api_cliente] ([id])
GO
ALTER TABLE [integracion].[api_token]  WITH CHECK ADD FOREIGN KEY([api_cliente_id])
REFERENCES [integracion].[api_cliente] ([id])
GO
ALTER TABLE [integracion].[dispositivo_pos]  WITH CHECK ADD FOREIGN KEY([almacen_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [integracion].[dispositivo_pos]  WITH CHECK ADD FOREIGN KEY([api_cliente_id])
REFERENCES [integracion].[api_cliente] ([id])
GO
ALTER TABLE [integracion].[dispositivo_pos]  WITH CHECK ADD FOREIGN KEY([caja_id])
REFERENCES [contabilidad].[caja] ([id])
GO
ALTER TABLE [integracion].[dispositivo_pos]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [integracion].[dispositivo_pos]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [integracion].[movimiento_caja_pos]  WITH CHECK ADD FOREIGN KEY([autorizado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [integracion].[movimiento_caja_pos]  WITH CHECK ADD FOREIGN KEY([factura_id])
REFERENCES [comercial].[factura_venta] ([id])
GO
ALTER TABLE [integracion].[movimiento_caja_pos]  WITH CHECK ADD FOREIGN KEY([sesion_caja_pos_id])
REFERENCES [integracion].[sesion_caja_pos] ([id])
GO
ALTER TABLE [integracion].[pos_rango_numeracion]  WITH CHECK ADD FOREIGN KEY([dispositivo_pos_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [integracion].[pos_sync_log]  WITH CHECK ADD FOREIGN KEY([dispositivo_pos_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [integracion].[pos_venta_pendiente]  WITH CHECK ADD FOREIGN KEY([dispositivo_pos_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [integracion].[pos_venta_pendiente]  WITH CHECK ADD FOREIGN KEY([factura_id])
REFERENCES [comercial].[factura_venta] ([id])
GO
ALTER TABLE [integracion].[pos_venta_pendiente]  WITH CHECK ADD FOREIGN KEY([sesion_caja_pos_id])
REFERENCES [integracion].[sesion_caja_pos] ([id])
GO
ALTER TABLE [integracion].[sesion_caja_pos]  WITH CHECK ADD FOREIGN KEY([asiento_cierre_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [integracion].[sesion_caja_pos]  WITH CHECK ADD FOREIGN KEY([cajero_id])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [integracion].[sesion_caja_pos]  WITH CHECK ADD FOREIGN KEY([dispositivo_pos_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [integracion].[sesion_caja_pos]  WITH CHECK ADD FOREIGN KEY([supervisor_conciliacion_id])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [integracion].[sesion_caja_pos]  WITH CHECK ADD  CONSTRAINT [fk_sesion_caja_caja] FOREIGN KEY([caja_id])
REFERENCES [contabilidad].[caja] ([id])
GO
ALTER TABLE [integracion].[sesion_caja_pos] CHECK CONSTRAINT [fk_sesion_caja_caja]
GO
ALTER TABLE [integracion].[webhook_entrega]  WITH CHECK ADD FOREIGN KEY([suscripcion_id])
REFERENCES [integracion].[webhook_suscripcion] ([id])
GO
ALTER TABLE [integracion].[webhook_suscripcion]  WITH CHECK ADD FOREIGN KEY([api_cliente_id])
REFERENCES [integracion].[api_cliente] ([id])
GO
ALTER TABLE [inventario].[almacen]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [inventario].[almacen]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [inventario].[conteo_fisico]  WITH CHECK ADD FOREIGN KEY([almacen_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [inventario].[conteo_fisico]  WITH CHECK ADD FOREIGN KEY([responsable_id])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [inventario].[conteo_fisico_detalle]  WITH CHECK ADD FOREIGN KEY([conteo_id])
REFERENCES [inventario].[conteo_fisico] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [inventario].[conteo_fisico_detalle]  WITH CHECK ADD FOREIGN KEY([movimiento_ajuste_id])
REFERENCES [inventario].[movimiento_inventario] ([id])
GO
ALTER TABLE [inventario].[conteo_fisico_detalle]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [inventario].[existencia]  WITH CHECK ADD FOREIGN KEY([almacen_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [inventario].[existencia]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [inventario].[existencia_lote]  WITH CHECK ADD  CONSTRAINT [FK__existencilote__almacen__3974] FOREIGN KEY([almacen_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [inventario].[existencia_lote] CHECK CONSTRAINT [FK__existencilote__almacen__3974]
GO
ALTER TABLE [inventario].[existencia_lote]  WITH CHECK ADD  CONSTRAINT [FK__existencilote__produ__39E2] FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [inventario].[existencia_lote] CHECK CONSTRAINT [FK__existencilote__produ__39E2]
GO
ALTER TABLE [inventario].[familia_producto]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [inventario].[familia_producto]  WITH CHECK ADD FOREIGN KEY([familia_padre_id])
REFERENCES [inventario].[familia_producto] ([id])
GO
ALTER TABLE [inventario].[lista_precio]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [inventario].[lista_precio_detalle]  WITH CHECK ADD FOREIGN KEY([lista_precio_id])
REFERENCES [inventario].[lista_precio] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [inventario].[lista_precio_detalle]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD FOREIGN KEY([almacen_origen_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD FOREIGN KEY([almacen_destino_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD FOREIGN KEY([tipo_movimiento_id])
REFERENCES [inventario].[tipo_movimiento] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD  CONSTRAINT [fk_mov_inv_dispositivo] FOREIGN KEY([dispositivo_pos_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [inventario].[movimiento_inventario] CHECK CONSTRAINT [fk_mov_inv_dispositivo]
GO
ALTER TABLE [inventario].[movimiento_inventario_detalle]  WITH CHECK ADD FOREIGN KEY([movimiento_id])
REFERENCES [inventario].[movimiento_inventario] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [inventario].[movimiento_inventario_detalle]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD FOREIGN KEY([cuenta_inventario_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD FOREIGN KEY([cuenta_costo_venta_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD FOREIGN KEY([cuenta_ingreso_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD FOREIGN KEY([familia_id])
REFERENCES [inventario].[familia_producto] ([id])
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD FOREIGN KEY([unidad_medida_id])
REFERENCES [inventario].[unidad_medida] ([id])
GO
ALTER TABLE [nucleo].[auditoria]  WITH CHECK ADD  CONSTRAINT [fk_auditoria_dispositivo] FOREIGN KEY([dispositivo_id])
REFERENCES [integracion].[dispositivo_pos] ([id])
GO
ALTER TABLE [nucleo].[auditoria] CHECK CONSTRAINT [fk_auditoria_dispositivo]
GO
ALTER TABLE [nucleo].[consecutivo]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[consecutivo]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [nucleo].[feedback]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[feedback]  WITH CHECK ADD FOREIGN KEY([usuario_id])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [nucleo].[notificacion]  WITH CHECK ADD  CONSTRAINT [FK_notificacion_entidad] FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[notificacion] CHECK CONSTRAINT [FK_notificacion_entidad]
GO
ALTER TABLE [nucleo].[notificacion]  WITH CHECK ADD  CONSTRAINT [FK_notificacion_usuario] FOREIGN KEY([usuario_id])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [nucleo].[notificacion] CHECK CONSTRAINT [FK_notificacion_usuario]
GO
ALTER TABLE [nucleo].[parametro_sistema]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[rol]  WITH CHECK ADD  CONSTRAINT [FK_Rol_Entidad] FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[rol] CHECK CONSTRAINT [FK_Rol_Entidad]
GO
ALTER TABLE [nucleo].[rol_permiso]  WITH CHECK ADD FOREIGN KEY([permiso_id])
REFERENCES [nucleo].[permiso] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [nucleo].[rol_permiso]  WITH CHECK ADD FOREIGN KEY([rol_id])
REFERENCES [nucleo].[rol] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [nucleo].[sucursal]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[usuario]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [nucleo].[usuario]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [nucleo].[usuario_rol]  WITH CHECK ADD FOREIGN KEY([asignado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [nucleo].[usuario_rol]  WITH CHECK ADD FOREIGN KEY([rol_id])
REFERENCES [nucleo].[rol] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [nucleo].[usuario_rol]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [nucleo].[usuario_rol]  WITH CHECK ADD FOREIGN KEY([usuario_id])
REFERENCES [nucleo].[usuario] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [produccion].[analisis_desviacion]  WITH CHECK ADD FOREIGN KEY([orden_produccion_id])
REFERENCES [produccion].[orden_produccion] ([id])
GO
ALTER TABLE [produccion].[equipo]  WITH CHECK ADD FOREIGN KEY([activo_fijo_id])
REFERENCES [contabilidad].[activo_fijo] ([id])
GO
ALTER TABLE [produccion].[equipo]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [produccion].[equipo]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [produccion].[ficha_costo]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [produccion].[ficha_costo]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [produccion].[ficha_costo]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [produccion].[lista_materiales]  WITH CHECK ADD FOREIGN KEY([producto_terminado_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [produccion].[lista_materiales_detalle]  WITH CHECK ADD FOREIGN KEY([lista_materiales_id])
REFERENCES [produccion].[lista_materiales] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [produccion].[lista_materiales_detalle]  WITH CHECK ADD FOREIGN KEY([producto_insumo_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [produccion].[mantenimiento_programado]  WITH CHECK ADD FOREIGN KEY([equipo_id])
REFERENCES [produccion].[equipo] ([id])
GO
ALTER TABLE [produccion].[mantenimiento_programado]  WITH CHECK ADD FOREIGN KEY([responsable_id])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [produccion].[merma]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [produccion].[merma]  WITH CHECK ADD FOREIGN KEY([orden_produccion_id])
REFERENCES [produccion].[orden_produccion] ([id])
GO
ALTER TABLE [produccion].[merma]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([almacen_insumos_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([almacen_producto_id])
REFERENCES [inventario].[almacen] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([asiento_consumo_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([asiento_terminado_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([creado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([ficha_costo_id])
REFERENCES [produccion].[ficha_costo] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([lista_materiales_id])
REFERENCES [produccion].[lista_materiales] ([id])
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD FOREIGN KEY([producto_terminado_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [produccion].[orden_produccion_consumo]  WITH CHECK ADD FOREIGN KEY([movimiento_inventario_id])
REFERENCES [inventario].[movimiento_inventario] ([id])
GO
ALTER TABLE [produccion].[orden_produccion_consumo]  WITH CHECK ADD FOREIGN KEY([orden_produccion_id])
REFERENCES [produccion].[orden_produccion] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [produccion].[orden_produccion_consumo]  WITH CHECK ADD FOREIGN KEY([producto_insumo_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [produccion].[plan_produccion]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [produccion].[plan_produccion]  WITH CHECK ADD FOREIGN KEY([presupuesto_id])
REFERENCES [contabilidad].[presupuesto] ([id])
GO
ALTER TABLE [produccion].[plan_produccion_detalle]  WITH CHECK ADD FOREIGN KEY([plan_id])
REFERENCES [produccion].[plan_produccion] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [produccion].[plan_produccion_detalle]  WITH CHECK ADD FOREIGN KEY([producto_id])
REFERENCES [inventario].[producto] ([id])
GO
ALTER TABLE [reportes].[indicador_valor]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [reportes].[indicador_valor]  WITH CHECK ADD FOREIGN KEY([indicador_id])
REFERENCES [reportes].[indicador] ([id])
GO
ALTER TABLE [reportes].[indicador_valor]  WITH CHECK ADD FOREIGN KEY([periodo_id])
REFERENCES [contabilidad].[periodo_contable] ([id])
GO
ALTER TABLE [reportes].[paquete_informacion]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [reportes].[paquete_informacion]  WITH CHECK ADD FOREIGN KEY([generado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [reportes].[paquete_informacion]  WITH CHECK ADD FOREIGN KEY([periodo_id])
REFERENCES [contabilidad].[periodo_contable] ([id])
GO
ALTER TABLE [reportes].[reporte_generado]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [reportes].[reporte_generado]  WITH CHECK ADD FOREIGN KEY([generado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [rrhh].[cargo]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [rrhh].[certificado_medico]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[concepto_nomina]  WITH CHECK ADD FOREIGN KEY([cuenta_contable_id])
REFERENCES [contabilidad].[cuenta_contable] ([id])
GO
ALTER TABLE [rrhh].[concepto_nomina]  WITH CHECK ADD  CONSTRAINT [FK__concepto___entidad_95020317] FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [rrhh].[concepto_nomina] CHECK CONSTRAINT [FK__concepto___entidad_95020317]
GO
ALTER TABLE [rrhh].[contrato_laboral]  WITH CHECK ADD FOREIGN KEY([cargo_id])
REFERENCES [rrhh].[cargo] ([id])
GO
ALTER TABLE [rrhh].[contrato_laboral]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[empleado]  WITH CHECK ADD FOREIGN KEY([cargo_id])
REFERENCES [rrhh].[cargo] ([id])
GO
ALTER TABLE [rrhh].[empleado]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [rrhh].[empleado]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [rrhh].[empleado]  WITH CHECK ADD  CONSTRAINT [FK_empleado_turno_trabajo] FOREIGN KEY([turno_trabajo_id])
REFERENCES [rrhh].[turno_trabajo] ([id])
GO
ALTER TABLE [rrhh].[empleado] CHECK CONSTRAINT [FK_empleado_turno_trabajo]
GO
ALTER TABLE [rrhh].[nomina_detalle]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[nomina_detalle]  WITH CHECK ADD FOREIGN KEY([periodo_nomina_id])
REFERENCES [rrhh].[periodo_nomina] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [rrhh].[nomina_detalle_concepto]  WITH CHECK ADD FOREIGN KEY([concepto_id])
REFERENCES [rrhh].[concepto_nomina] ([id])
GO
ALTER TABLE [rrhh].[nomina_detalle_concepto]  WITH CHECK ADD FOREIGN KEY([nomina_detalle_id])
REFERENCES [rrhh].[nomina_detalle] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [rrhh].[periodo_nomina]  WITH CHECK ADD FOREIGN KEY([aprobado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [rrhh].[periodo_nomina]  WITH CHECK ADD FOREIGN KEY([asiento_id])
REFERENCES [contabilidad].[asiento_contable] ([id])
GO
ALTER TABLE [rrhh].[periodo_nomina]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [rrhh].[plantilla_aprobada]  WITH CHECK ADD FOREIGN KEY([cargo_id])
REFERENCES [rrhh].[cargo] ([id])
GO
ALTER TABLE [rrhh].[plantilla_aprobada]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [rrhh].[plantilla_aprobada]  WITH CHECK ADD FOREIGN KEY([sucursal_id])
REFERENCES [nucleo].[sucursal] ([id])
GO
ALTER TABLE [rrhh].[registro_asistencia]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[registro_asistencia]  WITH CHECK ADD FOREIGN KEY([registrado_por])
REFERENCES [nucleo].[usuario] ([id])
GO
ALTER TABLE [rrhh].[registro_asistencia]  WITH CHECK ADD FOREIGN KEY([tipo_ausencia_id])
REFERENCES [rrhh].[tipo_ausencia] ([id])
GO
ALTER TABLE [rrhh].[registro_salario_tiempo_servicio]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[saldo_vacaciones]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[turno_trabajo]  WITH CHECK ADD  CONSTRAINT [FK_turno_trabajo_entidad] FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [rrhh].[turno_trabajo] CHECK CONSTRAINT [FK_turno_trabajo_entidad]
GO
ALTER TABLE [rrhh].[utile_responsabilidad]  WITH CHECK ADD FOREIGN KEY([empleado_id])
REFERENCES [rrhh].[empleado] ([id])
GO
ALTER TABLE [rrhh].[utile_responsabilidad]  WITH CHECK ADD FOREIGN KEY([entidad_id])
REFERENCES [nucleo].[entidad] ([id])
GO
ALTER TABLE [comercial].[cliente]  WITH CHECK ADD CHECK  (([segmento]='MAYORISTA' OR [segmento]='MINORISTA'))
GO
ALTER TABLE [comercial].[cliente]  WITH CHECK ADD CHECK  (([tipo_persona]='NATURAL' OR [tipo_persona]='JURIDICA'))
GO
ALTER TABLE [comercial].[contrato_economico]  WITH CHECK ADD  CONSTRAINT [chk_contrato_tercero] CHECK  (([tercero_tipo]='CLIENTE' AND [cliente_id] IS NOT NULL AND [proveedor_id] IS NULL OR [tercero_tipo]='PROVEEDOR' AND [proveedor_id] IS NOT NULL AND [cliente_id] IS NULL))
GO
ALTER TABLE [comercial].[contrato_economico] CHECK CONSTRAINT [chk_contrato_tercero]
GO
ALTER TABLE [comercial].[contrato_economico]  WITH CHECK ADD CHECK  (([estado]='RESCINDIDO' OR [estado]='VENCIDO' OR [estado]='VIGENTE'))
GO
ALTER TABLE [comercial].[contrato_economico]  WITH CHECK ADD CHECK  (([tercero_tipo]='PROVEEDOR' OR [tercero_tipo]='CLIENTE'))
GO
ALTER TABLE [comercial].[contrato_economico_suplemento]  WITH CHECK ADD  CONSTRAINT [CK_suplemento_estado] CHECK  (([estado]='ANULADO' OR [estado]='VIGENTE'))
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] CHECK CONSTRAINT [CK_suplemento_estado]
GO
ALTER TABLE [comercial].[contrato_economico_suplemento]  WITH CHECK ADD  CONSTRAINT [CK_suplemento_monto] CHECK  (([monto_total_nuevo]>=(0)))
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] CHECK CONSTRAINT [CK_suplemento_monto]
GO
ALTER TABLE [comercial].[contrato_economico_suplemento]  WITH CHECK ADD  CONSTRAINT [CK_suplemento_rango] CHECK  (([fecha_inicio] IS NULL OR [fecha_fin] IS NULL OR [fecha_fin]>[fecha_inicio]))
GO
ALTER TABLE [comercial].[contrato_economico_suplemento] CHECK CONSTRAINT [CK_suplemento_rango]
GO
ALTER TABLE [comercial].[devolucion_venta_detalle]  WITH CHECK ADD CHECK  (([cantidad_devuelta]>(0)))
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD CHECK  (([canal_venta]='MAYORISTA' OR [canal_venta]='API' OR [canal_venta]='POS' OR [canal_venta]='ERP'))
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD CHECK  (([estado]='DEVUELTA_TOTAL' OR [estado]='DEVUELTA_PARCIAL' OR [estado]='ANULADA' OR [estado]='EMITIDA' OR [estado]='BORRADOR'))
GO
ALTER TABLE [comercial].[factura_venta]  WITH CHECK ADD CHECK  (([tipo_venta]='MAYORISTA' OR [tipo_venta]='MINORISTA'))
GO
ALTER TABLE [comercial].[factura_venta_detalle]  WITH CHECK ADD CHECK  (([cantidad]>(0)))
GO
ALTER TABLE [comercial].[forma_pago_venta]  WITH CHECK ADD CHECK  (([forma_pago]='CREDITO' OR [forma_pago]='CHEQUE' OR [forma_pago]='TRANSFERENCIA_BANCARIA' OR [forma_pago]='ENZONA' OR [forma_pago]='TRANSFERMOVIL' OR [forma_pago]='EFECTIVO'))
GO
ALTER TABLE [comercial].[orden_compra]  WITH CHECK ADD CHECK  (([estado]='CANCELADA' OR [estado]='RECIBIDA_TOTAL' OR [estado]='RECIBIDA_PARCIAL' OR [estado]='APROBADA' OR [estado]='BORRADOR'))
GO
ALTER TABLE [comercial].[proveedor]  WITH CHECK ADD CHECK  (([tipo_persona]='NATURAL' OR [tipo_persona]='JURIDICA'))
GO
ALTER TABLE [comercial].[tope_precio_mfp]  WITH CHECK ADD  CONSTRAINT [chk_tope_alcance] CHECK  (([producto_id] IS NOT NULL OR [familia_id] IS NOT NULL))
GO
ALTER TABLE [comercial].[tope_precio_mfp] CHECK CONSTRAINT [chk_tope_alcance]
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD CHECK  (([estado]='TRASLADADO' OR [estado]='BAJA' OR [estado]='ACTIVO'))
GO
ALTER TABLE [contabilidad].[activo_fijo]  WITH CHECK ADD CHECK  (([metodo_depreciacion]='LINEA_RECTA'))
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD  CONSTRAINT [chk_cuadre] CHECK  (([total_debe]=[total_haber]))
GO
ALTER TABLE [contabilidad].[asiento_contable] CHECK CONSTRAINT [chk_cuadre]
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD CHECK  (([estado]='REVERTIDO' OR [estado]='CONTABILIZADO' OR [estado]='BORRADOR'))
GO
ALTER TABLE [contabilidad].[asiento_contable]  WITH CHECK ADD CHECK  (([modulo_origen]='ACTIVOS_FIJOS' OR [modulo_origen]='POS' OR [modulo_origen]='VENTAS' OR [modulo_origen]='COMPRAS' OR [modulo_origen]='PRODUCCION' OR [modulo_origen]='INVENTARIO' OR [modulo_origen]='NOMINA' OR [modulo_origen]='CONTABILIDAD'))
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH NOCHECK ADD  CONSTRAINT [chk_debe_o_haber] CHECK  (([debe]>(0) AND [haber]=(0) OR [haber]>(0) AND [debe]=(0)))
GO
ALTER TABLE [contabilidad].[asiento_detalle] NOCHECK CONSTRAINT [chk_debe_o_haber]
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH CHECK ADD CHECK  (([haber]>=(0)))
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH CHECK ADD CHECK  (([tercero_tipo]='EMPLEADO' OR [tercero_tipo]='PROVEEDOR' OR [tercero_tipo]='CLIENTE'))
GO
ALTER TABLE [contabilidad].[asiento_detalle]  WITH CHECK ADD CHECK  (([debe]>=(0)))
GO
ALTER TABLE [contabilidad].[cuenta_bancaria]  WITH CHECK ADD  CONSTRAINT [CK__cuenta_ba__moneda___74794745] CHECK  (([moneda]='CUP' OR [moneda]='MLC'))
GO
ALTER TABLE [contabilidad].[cuenta_bancaria] CHECK CONSTRAINT [CK__cuenta_ba__moneda___74794745]
GO
ALTER TABLE [contabilidad].[cuenta_bancaria]  WITH CHECK ADD  CONSTRAINT [CK__cuenta_ba__tipo___74794A92] CHECK  (([tipo_cuenta]='FISCAL' OR [tipo_cuenta]='CORRIENTE' OR [tipo_cuenta]='AHORRO'))
GO
ALTER TABLE [contabilidad].[cuenta_bancaria] CHECK CONSTRAINT [CK__cuenta_ba__tipo___74794A92]
GO
ALTER TABLE [contabilidad].[cuenta_contable]  WITH CHECK ADD  CONSTRAINT [CK__cuenta_co__clase__0A9D95DB] CHECK  (([clase]='ORDEN' OR [clase]='GASTOS' OR [clase]='INGRESOS' OR [clase]='PATRIMONIO' OR [clase]='PASIVO' OR [clase]='ACTIVO'))
GO
ALTER TABLE [contabilidad].[cuenta_contable] CHECK CONSTRAINT [CK__cuenta_co__clase__0A9D95DB]
GO
ALTER TABLE [contabilidad].[cuenta_contable]  WITH CHECK ADD CHECK  (([naturaleza]='ACREEDORA' OR [naturaleza]='DEUDORA'))
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar]  WITH CHECK ADD  CONSTRAINT [CK__cuenta_po__estad__0A888742] CHECK  (([estado]='PENDIENTE' OR [estado]='PARCIAL' OR [estado]='PAGADA' OR [estado]='INCOBRABLE'))
GO
ALTER TABLE [contabilidad].[cuenta_por_cobrar] CHECK CONSTRAINT [CK__cuenta_po__estad__0A888742]
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar]  WITH CHECK ADD  CONSTRAINT [CK__cuenta_po__estad__0B7CAB7B] CHECK  (([estado]='PENDIENTE' OR [estado]='PARCIAL' OR [estado]='PAGADA'))
GO
ALTER TABLE [contabilidad].[cuenta_por_pagar] CHECK CONSTRAINT [CK__cuenta_po__estad__0B7CAB7B]
GO
ALTER TABLE [contabilidad].[declaracion_jurada]  WITH CHECK ADD CHECK  (([estado]='VENCIDA' OR [estado]='PAGADA' OR [estado]='PRESENTADA' OR [estado]='PENDIENTE'))
GO
ALTER TABLE [contabilidad].[movimiento_bancario]  WITH CHECK ADD CHECK  (([tipo]='CREDITO' OR [tipo]='DEBITO'))
GO
ALTER TABLE [contabilidad].[pago_aplicado]  WITH CHECK ADD  CONSTRAINT [chk_pago_destino] CHECK  (([tipo]='COBRO' AND [cuenta_por_cobrar_id] IS NOT NULL AND [cuenta_por_pagar_id] IS NULL OR [tipo]='PAGO' AND [cuenta_por_pagar_id] IS NOT NULL AND [cuenta_por_cobrar_id] IS NULL))
GO
ALTER TABLE [contabilidad].[pago_aplicado] CHECK CONSTRAINT [chk_pago_destino]
GO
ALTER TABLE [contabilidad].[pago_aplicado]  WITH CHECK ADD CHECK  (([forma_pago]='CHEQUE' OR [forma_pago]='TRANSFERENCIA_BANCARIA' OR [forma_pago]='ENZONA' OR [forma_pago]='TRANSFERMOVIL' OR [forma_pago]='EFECTIVO'))
GO
ALTER TABLE [contabilidad].[pago_aplicado]  WITH CHECK ADD CHECK  (([tipo]='PAGO' OR [tipo]='COBRO'))
GO
ALTER TABLE [contabilidad].[periodo_contable]  WITH CHECK ADD CHECK  (([estado]='BLOQUEADO' OR [estado]='CERRADO' OR [estado]='ABIERTO'))
GO
ALTER TABLE [contabilidad].[periodo_contable]  WITH CHECK ADD CHECK  (([mes]>=(1) AND [mes]<=(12)))
GO
ALTER TABLE [contabilidad].[presupuesto]  WITH CHECK ADD CHECK  (([estado]='CERRADO' OR [estado]='APROBADO' OR [estado]='BORRADOR'))
GO
ALTER TABLE [contabilidad].[presupuesto_linea]  WITH CHECK ADD CHECK  (([mes]>=(1) AND [mes]<=(12)))
GO
ALTER TABLE [contabilidad].[tipo_obligacion_fiscal]  WITH CHECK ADD CHECK  (([periodicidad]='ANUAL' OR [periodicidad]='TRIMESTRAL' OR [periodicidad]='MENSUAL'))
GO
ALTER TABLE [integracion].[api_cliente]  WITH CHECK ADD CHECK  ((isjson([scopes])=(1)))
GO
ALTER TABLE [integracion].[api_cliente]  WITH CHECK ADD CHECK  (([tipo]='INTEGRACION_INTERNA' OR [tipo]='TERCERO' OR [tipo]='POS'))
GO
ALTER TABLE [integracion].[api_token]  WITH CHECK ADD CHECK  (([tipo]='REFRESH' OR [tipo]='ACCESS'))
GO
ALTER TABLE [integracion].[dispositivo_pos]  WITH CHECK ADD CHECK  (([estado]='MANTENIMIENTO' OR [estado]='INACTIVO' OR [estado]='ACTIVO'))
GO
ALTER TABLE [integracion].[movimiento_caja_pos]  WITH CHECK ADD CHECK  (([tipo]='DEVOLUCION' OR [tipo]='INGRESO_EFECTIVO' OR [tipo]='RETIRO_EFECTIVO' OR [tipo]='VENTA'))
GO
ALTER TABLE [integracion].[pos_rango_numeracion]  WITH CHECK ADD  CONSTRAINT [chk_rango] CHECK  (([numero_hasta]>[numero_desde] AND ([numero_siguiente_local]>=[numero_desde] AND [numero_siguiente_local]<=([numero_hasta]+(1)))))
GO
ALTER TABLE [integracion].[pos_rango_numeracion] CHECK CONSTRAINT [chk_rango]
GO
ALTER TABLE [integracion].[pos_sync_log]  WITH CHECK ADD CHECK  (([direccion]='POS_SERVIDOR' OR [direccion]='SERVIDOR_POS'))
GO
ALTER TABLE [integracion].[pos_sync_log]  WITH CHECK ADD CHECK  (([estado]='FALLIDO' OR [estado]='COMPLETADO' OR [estado]='EN_PROGRESO'))
GO
ALTER TABLE [integracion].[pos_sync_log]  WITH CHECK ADD CHECK  (([tipo_sync]='COMPLETA' OR [tipo_sync]='VENTAS_SUBIDA' OR [tipo_sync]='EXISTENCIAS' OR [tipo_sync]='PRECIOS' OR [tipo_sync]='CATALOGO'))
GO
ALTER TABLE [integracion].[pos_venta_pendiente]  WITH CHECK ADD CHECK  (([estado]='DUPLICADO' OR [estado]='ERROR' OR [estado]='PROCESADO' OR [estado]='PENDIENTE'))
GO
ALTER TABLE [integracion].[pos_venta_pendiente]  WITH CHECK ADD CHECK  ((isjson([payload_json])=(1)))
GO
ALTER TABLE [integracion].[sesion_caja_pos]  WITH CHECK ADD CHECK  (([estado]='CONCILIADA' OR [estado]='CERRADA' OR [estado]='ABIERTA'))
GO
ALTER TABLE [integracion].[webhook_entrega]  WITH CHECK ADD CHECK  ((isjson([payload_json])=(1)))
GO
ALTER TABLE [inventario].[conteo_fisico]  WITH CHECK ADD CHECK  (([estado]='CERRADO' OR [estado]='CONCILIADO' OR [estado]='EN_PROCESO'))
GO
ALTER TABLE [inventario].[conteo_fisico]  WITH CHECK ADD CHECK  (([tipo]='TOTAL' OR [tipo]='PARCIAL'))
GO
ALTER TABLE [inventario].[lista_precio]  WITH CHECK ADD CHECK  (([canal]='API' OR [canal]='POS' OR [canal]='ERP' OR [canal]='GENERAL'))
GO
ALTER TABLE [inventario].[movimiento_inventario]  WITH CHECK ADD CHECK  (([canal]='POS' OR [canal]='API' OR [canal]='ERP'))
GO
ALTER TABLE [inventario].[movimiento_inventario_detalle]  WITH CHECK ADD CHECK  (([cantidad]>(0)))
GO
ALTER TABLE [inventario].[producto]  WITH CHECK ADD CHECK  (([tipo]='MERCANCIA' OR [tipo]='SERVICIO' OR [tipo]='TERMINADO' OR [tipo]='EN_PROCESO' OR [tipo]='MATERIA_PRIMA'))
GO
ALTER TABLE [inventario].[tipo_movimiento]  WITH CHECK ADD CHECK  (([naturaleza]='SALIDA' OR [naturaleza]='ENTRADA'))
GO
ALTER TABLE [nucleo].[auditoria]  WITH CHECK ADD CHECK  (([accion]='REVERSION' OR [accion]='EXPORT' OR [accion]='LOGOUT' OR [accion]='LOGIN_FALLIDO' OR [accion]='LOGIN' OR [accion]='DELETE' OR [accion]='UPDATE' OR [accion]='INSERT'))
GO
ALTER TABLE [nucleo].[auditoria]  WITH CHECK ADD CHECK  (([canal]='POS' OR [canal]='API' OR [canal]='ERP'))
GO
ALTER TABLE [nucleo].[auditoria]  WITH CHECK ADD CHECK  (([valores_anteriores] IS NULL OR isjson([valores_anteriores])=(1)))
GO
ALTER TABLE [nucleo].[auditoria]  WITH CHECK ADD CHECK  (([valores_nuevos] IS NULL OR isjson([valores_nuevos])=(1)))
GO
ALTER TABLE [nucleo].[backup_log]  WITH CHECK ADD CHECK  (([estado]='FALLIDO' OR [estado]='COMPLETADO' OR [estado]='EN_PROGRESO'))
GO
ALTER TABLE [nucleo].[backup_log]  WITH CHECK ADD CHECK  (([tipo]='PRE_CIERRE' OR [tipo]='MANUAL' OR [tipo]='PROGRAMADO'))
GO
ALTER TABLE [nucleo].[feedback]  WITH CHECK ADD CHECK  (([estado]='ARCHIVADO' OR [estado]='RESUELTO' OR [estado]='REVISADO' OR [estado]='PENDIENTE'))
GO
ALTER TABLE [nucleo].[feedback]  WITH CHECK ADD CHECK  (([tipo]='SOPORTE' OR [tipo]='FELICITACION' OR [tipo]='ERROR' OR [tipo]='SUGERENCIA'))
GO
ALTER TABLE [nucleo].[parametro_sistema]  WITH CHECK ADD CHECK  (([tipo_dato]='DATE' OR [tipo_dato]='JSON' OR [tipo_dato]='BIT' OR [tipo_dato]='NUMERIC' OR [tipo_dato]='STRING'))
GO
ALTER TABLE [nucleo].[sucursal]  WITH CHECK ADD CHECK  (([tipo]='MIXTO' OR [tipo]='PRODUCCION' OR [tipo]='TIENDA' OR [tipo]='ALMACEN' OR [tipo]='OFICINA'))
GO
ALTER TABLE [produccion].[analisis_desviacion]  WITH CHECK ADD CHECK  (([componente]='GASTOS_INDIRECTOS' OR [componente]='MANO_OBRA' OR [componente]='MATERIA_PRIMA'))
GO
ALTER TABLE [produccion].[equipo]  WITH CHECK ADD CHECK  (([estado]='FUERA_SERVICIO' OR [estado]='MANTENIMIENTO' OR [estado]='OPERATIVO'))
GO
ALTER TABLE [produccion].[ficha_costo]  WITH CHECK ADD CHECK  (([estado]='OBSOLETA' OR [estado]='VIGENTE' OR [estado]='BORRADOR'))
GO
ALTER TABLE [produccion].[lista_materiales_detalle]  WITH CHECK ADD CHECK  (([cantidad_requerida]>(0)))
GO
ALTER TABLE [produccion].[mantenimiento_programado]  WITH CHECK ADD CHECK  (([estado]='VENCIDO' OR [estado]='EJECUTADO' OR [estado]='PROGRAMADO'))
GO
ALTER TABLE [produccion].[mantenimiento_programado]  WITH CHECK ADD CHECK  (([tipo]='CORRECTIVO' OR [tipo]='PREVENTIVO'))
GO
ALTER TABLE [produccion].[merma]  WITH CHECK ADD CHECK  (([causa]='OTRO' OR [causa]='ERROR_OPERATIVO' OR [causa]='VENCIMIENTO' OR [causa]='DANO' OR [causa]='PROCESO_NORMAL'))
GO
ALTER TABLE [produccion].[orden_produccion]  WITH CHECK ADD CHECK  (([estado]='CANCELADA' OR [estado]='TERMINADA' OR [estado]='EN_PROCESO' OR [estado]='PLANIFICADA'))
GO
ALTER TABLE [produccion].[plan_produccion]  WITH CHECK ADD CHECK  (([estado]='EJECUTADO' OR [estado]='APROBADO' OR [estado]='BORRADOR'))
GO
ALTER TABLE [produccion].[plan_produccion]  WITH CHECK ADD CHECK  (([mes]>=(1) AND [mes]<=(12)))
GO
ALTER TABLE [reportes].[indicador]  WITH CHECK ADD CHECK  (([categoria]='OPERATIVO' OR [categoria]='PRESUPUESTO' OR [categoria]='ACTIVIDAD' OR [categoria]='RENTABILIDAD' OR [categoria]='LIQUIDEZ'))
GO
ALTER TABLE [reportes].[paquete_informacion]  WITH CHECK ADD CHECK  (([estado]='ENVIADO' OR [estado]='GENERADO'))
GO
ALTER TABLE [reportes].[paquete_informacion]  WITH CHECK ADD CHECK  (([tipo]='MFP' OR [tipo]='ONAT' OR [tipo]='DIRECCION'))
GO
ALTER TABLE [reportes].[reporte_generado]  WITH CHECK ADD CHECK  (([formato]='CSV' OR [formato]='XLSX' OR [formato]='PDF'))
GO
ALTER TABLE [reportes].[reporte_generado]  WITH CHECK ADD CHECK  (([parametros_json] IS NULL OR isjson([parametros_json])=(1)))
GO
ALTER TABLE [rrhh].[concepto_nomina]  WITH CHECK ADD CHECK  (([tipo]='APORTE_PATRONAL' OR [tipo]='DEDUCCION' OR [tipo]='DEVENGO'))
GO
ALTER TABLE [rrhh].[contrato_laboral]  WITH CHECK ADD CHECK  (([estado]='RESCINDIDO' OR [estado]='FINALIZADO' OR [estado]='VIGENTE'))
GO
ALTER TABLE [rrhh].[contrato_laboral]  WITH CHECK ADD CHECK  (([tipo_contrato]='PRUEBA' OR [tipo_contrato]='INDETERMINADO' OR [tipo_contrato]='DETERMINADO'))
GO
ALTER TABLE [rrhh].[empleado]  WITH CHECK ADD CHECK  (([estado]='BAJA' OR [estado]='LICENCIA' OR [estado]='ACTIVO'))
GO
ALTER TABLE [rrhh].[empleado]  WITH CHECK ADD CHECK  (([sexo]='F' OR [sexo]='M'))
GO
ALTER TABLE [rrhh].[periodo_nomina]  WITH CHECK ADD CHECK  (([estado]='PAGADA' OR [estado]='CONTABILIZADA' OR [estado]='APROBADA' OR [estado]='CALCULADA' OR [estado]='PRENOMINA'))
GO
ALTER TABLE [rrhh].[periodo_nomina]  WITH CHECK ADD CHECK  (([tipo]='EXTRAORDINARIA' OR [tipo]='QUINCENAL' OR [tipo]='MENSUAL'))
GO
ALTER TABLE [rrhh].[periodo_nomina]  WITH CHECK ADD CHECK  (([mes]>=(1) AND [mes]<=(12)))
GO
ALTER TABLE [rrhh].[registro_salario_tiempo_servicio]  WITH CHECK ADD CHECK  (([mes]>=(1) AND [mes]<=(12)))
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'El "cliente mostrador" del POS (venta anónima) se modela como registro fijo con nit_o_ci=NULL, nombre_razon_social=''CONSUMIDOR FINAL''.' , @level0type=N'SCHEMA',@level0name=N'comercial', @level1type=N'TABLE',@level1name=N'cliente'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-51: asignado vía nucleo.consecutivo con SELECT...FOR UPDATE; numeración reservada por sesión offline (ver integracion.pos_venta_pendiente) para sobrevivir cortes de red del POS.' , @level0type=N'SCHEMA',@level0name=N'comercial', @level1type=N'TABLE',@level1name=N'factura_venta', @level2type=N'COLUMN',@level2name=N'numero_factura'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RF-12: un asiento contabilizado jamás se edita ni elimina; solo se revierte mediante un nuevo asiento de ajuste enlazado aquí.' , @level0type=N'SCHEMA',@level0name=N'contabilidad', @level1type=N'TABLE',@level1name=N'asiento_contable', @level2type=N'COLUMN',@level2name=N'asiento_reversion_id'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Alternativa a reservar consecutivos: el servidor asigna bloques (p.ej. 1000 números) a cada terminal al sincronizar. El terminal numera localmente dentro de su rango incluso sin conexión, preservando RNF-51 (sin duplicados ni saltos) sin depender de la red para cada venta.' , @level0type=N'SCHEMA',@level0name=N'integracion', @level1type=N'TABLE',@level1name=N'pos_rango_numeracion'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-02/RNF-50: la app POS crea el registro localmente con idempotency_key propio y hace upsert al reconectar. El worker de sincronización procesa PENDIENTE -> crea factura_venta -> marca PROCESADO. Reintentos seguros gracias a la clave única (dispositivo, idempotency_key).' , @level0type=N'SCHEMA',@level0name=N'integracion', @level1type=N'TABLE',@level1name=N'pos_venta_pendiente'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Una caja puede tener una única sesión ABIERTA, compartida por todos sus dispositivos (UNIQUE(caja_id) WHERE estado=''ABIERTA'').' , @level0type=N'SCHEMA',@level0name=N'integracion', @level1type=N'TABLE',@level1name=N'sesion_caja_pos'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Actualizar con SELECT ... FOR UPDATE dentro de la transacción para evitar saltos/duplicados concurrentes (POS + ERP simultáneo).' , @level0type=N'SCHEMA',@level0name=N'nucleo', @level1type=N'TABLE',@level1name=N'consecutivo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tasas fiscales, escalas salariales, % vacaciones, etc. Versionado por vigencia para resistir cambios normativos frecuentes del MFP/ONAT.' , @level0type=N'SCHEMA',@level0name=N'nucleo', @level1type=N'TABLE',@level1name=N'parametro_sistema'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-40: versionada para permitir redefinición ágil ante cambios de precios de insumos.' , @level0type=N'SCHEMA',@level0name=N'produccion', @level1type=N'TABLE',@level1name=N'ficha_costo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-20: acceso restringido — datos de salud del trabajador, solo RR.HH. y dirección.' , @level0type=N'SCHEMA',@level0name=N'rrhh', @level1type=N'TABLE',@level1name=N'certificado_medico'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RNF-22: histórico salarial inalterable — no se actualiza tras CONTABILIZADA, solo se referencia para reportes probatorios.' , @level0type=N'SCHEMA',@level0name=N'rrhh', @level1type=N'TABLE',@level1name=N'nomina_detalle_concepto'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Equivalente a modelo SC-4-08 u oficial vigente MTSS.' , @level0type=N'SCHEMA',@level0name=N'rrhh', @level1type=N'TABLE',@level1name=N'registro_salario_tiempo_servicio'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Conciliación de arqueo de caja POS: diferencia declarada vs sistema y su estado contable (item 21).' , @level0type=N'SCHEMA',@level0name=N'contabilidad', @level1type=N'VIEW',@level1name=N'v_arqueo_caja'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Base para balance general y estado de resultados. Los reportes NCC (RF-13) se construyen agregando esta vista por clase de cuenta.' , @level0type=N'SCHEMA',@level0name=N'contabilidad', @level1type=N'VIEW',@level1name=N'v_saldo_cuenta'
GO
USE [master]
GO
ALTER DATABASE [VERCOMERP] SET  READ_WRITE 
GO
