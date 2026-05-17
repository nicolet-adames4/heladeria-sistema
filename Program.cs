using Heladeria;
using Heladeria.Models;

Database.Inicializar();

bool salir = false;
while (!salir)
{
    Console.Clear();
    Console.WriteLine("==============================");
    Console.WriteLine("   HELADERIA - SISTEMA  ");
    Console.WriteLine("==============================");
    Console.WriteLine("1. Ver productos");
    Console.WriteLine("2. Agregar producto");
    Console.WriteLine("3. Actualizar precios");
    Console.WriteLine("4. Registrar venta");
    Console.WriteLine("5. Ver ventas");
    Console.WriteLine("6. Reporte mensual");
    Console.WriteLine("7. Salir");
    Console.WriteLine("==============================");
    Console.Write("Opcion: ");

    switch (Console.ReadLine())
    {
        case "1": VerProductos();      break;
        case "2": AgregarProducto();   break;
        case "3": ActualizarPrecios(); break;
        case "4": RegistrarVenta();    break;
        case "5": VerVentas();         break;
        case "6": ReporteMensual();    break;
        case "75": salir = true;        break;
        default:
            Console.WriteLine("Opcion invalida.");
            break;
    }

    if (!salir)
    {
        Console.WriteLine("\nPresione ENTER para continuar...");
        Console.ReadLine();
    }
}

void VerProductos()
{
    Console.Clear();
    Console.WriteLine("\n--- PRODUCTOS ---\n");
    var lista = Database.ObtenerProductos();

    foreach (var p in lista)
        Console.WriteLine(p);
}

void AgregarProducto()
{
    Console.Clear();
    Console.WriteLine("\n--- AGREGAR PRODUCTO ---\n");

    Console.WriteLine("Tipo: 1. Paleta   2. Helado");
    Console.Write("Seleccione: ");
    string tipo = Console.ReadLine() == "1" ? "Paleta" : "Helado";

    Console.Write("Nombre del producto: ");
    string nombre = Console.ReadLine() ?? "";

    Console.Write("Precio de compra RD$: ");
    decimal.TryParse(Console.ReadLine(), out decimal compra);

    Console.Write("Precio de venta RD$: ");
    decimal.TryParse(Console.ReadLine(), out decimal venta);

    if (venta <= compra)
    {
        Console.WriteLine("El precio de venta debe ser mayor al de compra.");
        return;
    }

    Database.AgregarProducto(new Producto
    {
        Tipo         = tipo,
        Nombre       = nombre,
        PrecioCompra = compra,
        PrecioVenta  = venta
    });

    Console.WriteLine("Producto agregado.");
}

void ActualizarPrecios()
{
    Console.Clear();
    VerProductos();

    Console.Write("\nID del producto: ");
    int.TryParse(Console.ReadLine(), out int id);

    Console.Write("Nuevo precio de compra RD$: ");
    decimal.TryParse(Console.ReadLine(), out decimal compra);

    Console.Write("Nuevo precio de venta RD$: ");
    decimal.TryParse(Console.ReadLine(), out decimal venta);

    if (venta <= compra)
    {
        Console.WriteLine("El precio de venta debe ser mayor al de compra.");
        return;
    }

    Database.ActualizarPrecios(id, compra, venta);
    Console.WriteLine("Precios actualizados.");
}
// Modulo de ventas: permite registrar productos vendidos y calcular el total
void RegistrarVenta()

{
    Console.Clear();
    Console.WriteLine("\n--- REGISTRAR VENTA ---\n");
    VerProductos();

    var detalles = new List<DetalleVenta>();

    while (true)
    {
        Console.Write("\nID del producto (0 para terminar): ");
        int.TryParse(Console.ReadLine(), out int id);
        if (id == 0) break;

        var producto = Database.ObtenerProducto(id);
        if (producto == null)
        {
            Console.WriteLine("Producto no encontrado.");
            continue;
        }

        Console.Write("Cantidad: ");
        int.TryParse(Console.ReadLine(), out int cantidad);
        if (cantidad <= 0) continue;

        detalles.Add(new DetalleVenta
        {
            ProductoId   = producto.Id,
            Nombre       = producto.Nombre,
            Tipo         = producto.Tipo,
            Cantidad     = cantidad,
            PrecioVenta  = producto.PrecioVenta,
            PrecioCompra = producto.PrecioCompra
        });

        Console.WriteLine($"Agregado: {producto.Nombre} x{cantidad}");
    }

    if (detalles.Count == 0)
    {
        Console.WriteLine("Venta cancelada.");
        return;
    }

    var venta = new Venta { Fecha = DateTime.Now, Detalles = detalles };
    int ventaId = Database.RegistrarVenta(venta);

    Console.WriteLine($"\nVenta #{ventaId} registrada.");
    Console.WriteLine("--- Detalle ---");
    foreach (var d in detalles)
        Console.WriteLine(d);

    Console.WriteLine($"\nTotal  : RD${venta.Total:N2}");
    Console.WriteLine($"Ganancia: RD${venta.Ganancia:N2}");
}

void VerVentas()
{
    Console.Clear();
    Console.WriteLine("\n--- HISTORIAL DE VENTAS ---\n");
    var ventas = Database.ObtenerVentas();

    if (ventas.Count == 0)
    {
        Console.WriteLine("No hay ventas registradas.");
        return;
    }

    foreach (var v in ventas)
    {
        Console.WriteLine($"Venta #{v.Id} - {v.Fecha:dd/MM/yyyy HH:mm} | Total: RD${v.Total:N2} | Ganancia: RD${v.Ganancia:N2}");
        foreach (var d in v.Detalles)
            Console.WriteLine(d);
        Console.WriteLine();
    }
}

void ReporteMensual()
{
    Console.Clear();
    Console.WriteLine("\n--- REPORTE MENSUAL ---\n");

    Console.Write("Mes (1-12): ");
    int.TryParse(Console.ReadLine(), out int mes);

    Console.Write("Año: ");
    int.TryParse(Console.ReadLine(), out int anio);

    var ventas = Database.ObtenerVentasPorMes(mes, anio);

    if (ventas.Count == 0)
    {
        Console.WriteLine("No hay ventas para ese mes.");
        return;
    }

    decimal totalVendido  = ventas.Sum(v => v.Total);
    decimal totalGanancia = ventas.Sum(v => v.Ganancia);
    int     totalUnidades = ventas.SelectMany(v => v.Detalles).Sum(d => d.Cantidad);

    decimal ventasPaletas   = ventas.SelectMany(v => v.Detalles).Where(d => d.Tipo == "Paleta").Sum(d => d.Subtotal);
    decimal ventasHelados   = ventas.SelectMany(v => v.Detalles).Where(d => d.Tipo == "Helado").Sum(d => d.Subtotal);
    decimal gananciaPaletas = ventas.SelectMany(v => v.Detalles).Where(d => d.Tipo == "Paleta").Sum(d => d.Ganancia);
    decimal gananciaHelados = ventas.SelectMany(v => v.Detalles).Where(d => d.Tipo == "Helado").Sum(d => d.Ganancia);

    var nombreMes = new DateTime(anio, mes, 1)
        .ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));

    Console.WriteLine($"==============================");
    Console.WriteLine($"  REPORTE: {nombreMes.ToUpper()}");
    Console.WriteLine($"==============================");
    Console.WriteLine($"  Ventas realizadas : {ventas.Count}");
    Console.WriteLine($"  Unidades vendidas : {totalUnidades}");
    Console.WriteLine($"------------------------------");
    Console.WriteLine($"  Total vendido     : RD${totalVendido:N2}");
    Console.WriteLine($"  Ventas paletas    : RD${ventasPaletas:N2}");
    Console.WriteLine($"  Ventas helados    : RD${ventasHelados:N2}");
    Console.WriteLine($"------------------------------");
    Console.WriteLine($"  Ganancia neta     : RD${totalGanancia:N2}");
    Console.WriteLine($"  Ganancia paletas  : RD${gananciaPaletas:N2}");
    Console.WriteLine($"  Ganancia helados  : RD${gananciaHelados:N2}");
    Console.WriteLine($"==============================");
}