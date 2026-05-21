namespace SuperBodega.Web.Modelos;

public class VentaDTO
{
    public int ClienteId { get; set; }

    public List<ProductoVentaDTO> Productos { get; set; }
        = new();
}

public class ProductoVentaDTO
{
    public int ProductoId { get; set; }

    public int Cantidad { get; set; }
}