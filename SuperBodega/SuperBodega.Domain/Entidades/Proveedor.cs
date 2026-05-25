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

    public ICollection<Producto>? Productos
    {
        get; set;
    }
}