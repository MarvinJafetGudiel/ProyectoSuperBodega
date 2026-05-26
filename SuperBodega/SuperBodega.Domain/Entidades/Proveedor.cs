namespace SuperBodega.Domain.Entidades;

public class Proveedor
{
    public int Id { get; set; }

    public string Nombre { get; set; }
        = string.Empty;

    public string Correo { get; set; }
        = string.Empty;

    public string Telefono { get; set; }
        = string.Empty;

    public List<Compra>? Compras { get; set; }

    public ICollection<Producto>? Productos
    {
        get; set;
    }
}