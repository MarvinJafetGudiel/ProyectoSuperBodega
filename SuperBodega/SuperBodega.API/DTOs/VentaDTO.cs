namespace SuperBodega.API.DTOs;

public class VentaDTO
{
	public int ClienteId { get; set; }

	public List<DetalleVentaDTO> Productos { get; set; } = new();
}