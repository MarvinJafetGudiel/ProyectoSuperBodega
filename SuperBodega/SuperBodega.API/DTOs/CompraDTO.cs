namespace SuperBodega.API.DTOs;

public class CompraDTO
{
    public int ProveedorId { get; set; }

    public List<ProductoCompraDTO> Productos { get; set; }
        = new();
}

public class ProductoCompraDTO
{
    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}