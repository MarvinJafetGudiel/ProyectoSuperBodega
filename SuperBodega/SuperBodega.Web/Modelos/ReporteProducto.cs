namespace SuperBodega.Web.Modelos;

public class ReporteProducto
{
	public string Producto { get; set; }
		= string.Empty;

	public int CantidadVendida { get; set; }

	public decimal TotalGenerado { get; set; }
}