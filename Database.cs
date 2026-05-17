using Dapper;
using Microsoft.Data.Sqlite;
using Heladeria.Models;

namespace Heladeria;

public static class Database
{
    const string Conexion = "Data Source=heladeria.db";

    public static SqliteConnection Conectar() => new(Conexion);

    // ── Crear tablas y datos de ejemplo ──────────────────────────────────
    public static void Inicializar()
    {
        using var db = Conectar();

        db.Execute(@"
            CREATE TABLE IF NOT EXISTS Productos (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                Tipo         TEXT    NOT NULL,
                Nombre       TEXT    NOT NULL,
                PrecioCompra REAL    NOT NULL,
                PrecioVenta  REAL    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Ventas (
                Id    INTEGER  PRIMARY KEY AUTOINCREMENT,
                Fecha DATETIME NOT NULL
            );

            CREATE TABLE IF NOT EXISTS DetallesVenta (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                VentaId      INTEGER NOT NULL,
                ProductoId   INTEGER NOT NULL,
                Nombre       TEXT    NOT NULL,
                Tipo         TEXT    NOT NULL,
                Cantidad     INTEGER NOT NULL,
                PrecioVenta  REAL    NOT NULL,
                PrecioCompra REAL    NOT NULL
            );
        ");

        // Solo inserta si la tabla está vacía
        int total = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Productos;");
        if (total == 0)
        {
            db.Execute(@"
                INSERT INTO Productos (Tipo, Nombre, PrecioCompra, PrecioVenta) VALUES
                ('Paleta', 'Paleta Chocolate',             25, 50),
                ('Paleta', 'Paleta Bizcocho',              25, 50),
                ('Paleta', 'Paleta Vainilla',              25, 50),
                ('Paleta', 'Paleta Revestido de Chocolate',30, 60),
                ('Helado', 'Helado Choco Crema',           40, 80),
                ('Helado', 'Helado Chinola Crema',         40, 80),
                ('Helado', 'Helado Dulce de Leche',        40, 80),
                ('Helado', 'Helado Aguacate Crema',        45, 85);
            ");
        }
    }

    // ── Productos ─────────────────────────────────────────────────────────
    public static List<Producto> ObtenerProductos()
    {
        using var db = Conectar();
        return db.Query<Producto>("SELECT * FROM Productos;").ToList();
    }

    public static Producto? ObtenerProducto(int id)
    {
        using var db = Conectar();
        return db.QueryFirstOrDefault<Producto>(
            "SELECT * FROM Productos WHERE Id = @Id;", new { Id = id });
    }

    public static void AgregarProducto(Producto p)
    {
        using var db = Conectar();
        db.Execute(
            "INSERT INTO Productos (Tipo, Nombre, PrecioCompra, PrecioVenta) VALUES (@Tipo, @Nombre, @PrecioCompra, @PrecioVenta);",
            p);
    }

    public static void ActualizarPrecios(int id, decimal compra, decimal venta)
    {
        using var db = Conectar();
        db.Execute(
            "UPDATE Productos SET PrecioCompra = @Compra, PrecioVenta = @Venta WHERE Id = @Id;",
            new { Id = id, Compra = compra, Venta = venta });
    }

    // ── Ventas ────────────────────────────────────────────────────────────
    public static int RegistrarVenta(Venta venta)
    {
        using var db = Conectar();
        db.Open();
        using var tx = db.BeginTransaction();

        int ventaId = db.ExecuteScalar<int>(
            "INSERT INTO Ventas (Fecha) VALUES (@Fecha); SELECT last_insert_rowid();",
            new { venta.Fecha }, tx);

        foreach (var d in venta.Detalles)
        {
            d.VentaId = ventaId;
            db.Execute(@"
                INSERT INTO DetallesVenta (VentaId, ProductoId, Nombre, Tipo, Cantidad, PrecioVenta, PrecioCompra)
                VALUES (@VentaId, @ProductoId, @Nombre, @Tipo, @Cantidad, @PrecioVenta, @PrecioCompra);", d, tx);
        }

        tx.Commit();
        return ventaId;
    }

    public static List<Venta> ObtenerVentas()
    {
        using var db = Conectar();
        var ventas = db.Query<Venta>("SELECT * FROM Ventas ORDER BY Fecha DESC;").ToList();
        foreach (var v in ventas)
            v.Detalles = db.Query<DetalleVenta>(
                "SELECT * FROM DetallesVenta WHERE VentaId = @Id;", new { v.Id }).ToList();
        return ventas;
    }

    public static List<Venta> ObtenerVentasPorMes(int mes, int anio)
    {
        using var db = Conectar();
        var ventas = db.Query<Venta>(@"
            SELECT * FROM Ventas
            WHERE strftime('%m', Fecha) = @Mes
              AND strftime('%Y', Fecha) = @Anio
            ORDER BY Fecha DESC;",
            new { Mes = mes.ToString("D2"), Anio = anio.ToString() }).ToList();

        foreach (var v in ventas)
            v.Detalles = db.Query<DetalleVenta>(
                "SELECT * FROM DetallesVenta WHERE VentaId = @Id;", new { v.Id }).ToList();
        return ventas;
    }
}