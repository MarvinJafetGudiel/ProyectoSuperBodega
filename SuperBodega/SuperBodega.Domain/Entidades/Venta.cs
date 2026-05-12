namespace SuperBodega.Domain.Entidades;

public class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "Pendiente";

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public ICollection<DetalleVenta>? Detalles { get; set; }
}