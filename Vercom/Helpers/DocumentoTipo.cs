namespace Vercom.Helpers;

public static class DocumentoTipo
{
    public const string FacturaVenta = "FACTURA_VENTA";
    public const string OrdenCompra = "ORDEN_COMPRA";
    public const string OrdenProduccion = "ORDEN_PRODUCCION";
    public const string AsientoContable = "ASIENTO_CONTABLE";
    public const string ValeEntrada = "VALE_ENTRADA";
    public const string ValeSalida = "VALE_SALIDA";

    public static readonly (string Codigo, string Nombre)[] Catalogo =
    {
        (FacturaVenta, "Factura de Venta"),
        (OrdenCompra, "Orden de Compra"),
        (OrdenProduccion, "Orden de Producción"),
        (AsientoContable, "Asiento Contable"),
        (ValeEntrada, "Vale de Entrada"),
        (ValeSalida, "Vale de Salida")
    };
}