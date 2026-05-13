using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.DTOs;
using SuperBodega.Domain.Entidades;
using SuperBodega.Infrastructure.Datos;
using SuperBodega.API.Mensajeria;
using System.Text.Json;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;
    private readonly RabbitMQProductor _productor;

    public VentasController(ApplicationDbContext contexto, RabbitMQProductor productor)
    {
        _contexto = contexto;
        _productor = productor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Venta>>> ObtenerVentas()
    {
        var ventas = await _contexto.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles!)
            .ThenInclude(d => d.Producto)
            .ToListAsync();

        return Ok(ventas);
    }

    [HttpPost]
    public async Task<ActionResult> CrearVenta(VentaDTO dto)
    {
        var clienteExiste = await _contexto.Clientes
            .AnyAsync(c => c.Id == dto.ClienteId);

        if (!clienteExiste)
            return BadRequest("Cliente no existe");

        decimal total = 0;
        var detalles = new List<DetalleVenta>();

        foreach (var item in dto.Productos)
        {
            var producto = await _contexto.Productos
                .FirstOrDefaultAsync(p => p.Id == item.ProductoId);

            if (producto == null)
                return BadRequest($"Producto {item.ProductoId} no existe");

            if (producto.Stock < item.Cantidad)
                return BadRequest($"Stock insuficiente para {producto.Nombre}");

            producto.Stock -= item.Cantidad;

            var detalle = new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio
            };

            total += producto.Precio * item.Cantidad;
            detalles.Add(detalle);
        }

        var venta = new Venta
        {
            ClienteId = dto.ClienteId,
            Fecha = DateTime.UtcNow,
            Estado = "Recibido",
            Total = total,
            Detalles = detalles
        };

        _contexto.Ventas.Add(venta);
        await _contexto.SaveChangesAsync();

        var mensaje = JsonSerializer.Serialize(venta);

        await _productor.Enviar(mensaje);

        return Ok(new
        {
            mensaje = "Venta enviada a RabbitMQ",
            venta
        });
    }
}