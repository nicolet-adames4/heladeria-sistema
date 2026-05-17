namespace Heladeria.Models;

public class Producto
{
    public int     Id           { get; set; }
    public string  Tipo         { get; set; } = ""; // "Paleta" o "Helado"
    public string  Nombre       { get; set; } = "";
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta  { get; set; }

    public decimal Ganancia => PrecioVenta - PrecioCompra;

    public override string ToString() =>
        $"[{Id}] {Tipo} - {Nombre} | Compra: RD${PrecioCompra:N2} | Venta: RD${PrecioVenta:N2} | Ganancia: RD${Ganancia:N2}";
}