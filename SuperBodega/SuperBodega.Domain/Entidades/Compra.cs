namespace SuperBodega.Domain.Entidades;

public class Compra
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public int ProveedorId { get; set; }

    public Proveedor? Proveedor { get; set; }

    public List<DetalleCompra>? Detalles { get; set; }
}