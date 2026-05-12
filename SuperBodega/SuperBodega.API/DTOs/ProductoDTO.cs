namespace SuperBodega.API.DTOs;

public class ProductoDTO
{
    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public int CategoriaId { get; set; }
}