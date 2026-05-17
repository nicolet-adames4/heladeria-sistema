namespace Heladeria.Models;

public class Venta
{
    public int      Id      { get; set; }
    public DateTime Fecha   { get; set; } = DateTime.Now;

    public List<DetalleVenta> Detalles { get; set; } = new();

    public decimal Total    => Detalles.Sum(d => d.Subtotal);
    public decimal Ganancia => Detalles.Sum(d => d.Ganancia);
}