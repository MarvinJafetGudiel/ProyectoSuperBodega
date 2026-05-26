using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.DTOs;
using SuperBodega.Domain.Entidades;
using SuperBodega.Infrastructure.Datos;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    public ComprasController(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Compra>>> Obtener()
    {
        var compras = await _contexto.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles!)
            .ThenInclude(d => d.Producto)
            .ToListAsync();

        return Ok(compras);
    }

    [HttpPost]
    public async Task<ActionResult> Crear(CompraDTO dto)
    {
        var proveedorExiste = await _contexto.Proveedores
            .AnyAsync(p => p.Id == dto.ProveedorId);

        if (!proveedorExiste)
            return BadRequest("Proveedor no existe");

        decimal total = 0;

        var detalles = new List<DetalleCompra>();

        foreach (var item in dto.Productos)
        {
            var producto = await _contexto.Productos
                .FirstOrDefaultAsync(
                    p => p.Id == item.ProductoId);

            if (producto == null)
                return BadRequest(
                    $"Producto {item.ProductoId} no existe");

            // AUMENTAR STOCK
            producto.Stock += item.Cantidad;

            var detalle = new DetalleCompra
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            };

            total +=
                item.Cantidad *
                item.PrecioUnitario;

            detalles.Add(detalle);
        }

        var compra = new Compra
        {
            Fecha = DateTime.UtcNow,
            ProveedorId = dto.ProveedorId,
            Total = total,
            Detalles = detalles
        };

        _contexto.Compras.Add(compra);

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Compra registrada",
            compra
        });
    }
}