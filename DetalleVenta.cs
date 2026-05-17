namespace Heladeria.Models;

public class DetalleVenta
{
    public int     Id           { get; set; }
    public int     VentaId      { get; set; }
    public int     ProductoId   { get; set; }
    public string  Nombre       { get; set; } = "";
    public string  Tipo         { get; set; } = "";
    public int     Cantidad     { get; set; }
    public decimal PrecioVenta  { get; set; }
    public decimal PrecioCompra { get; set; }

    public decimal Subtotal => Cantidad * PrecioVenta;
    public decimal Ganancia => Cantidad * (PrecioVenta - PrecioCompra);

    public override string ToString() =>
        $"  {Nombre} x{Cantidad} = RD${Subtotal:N2}";
}