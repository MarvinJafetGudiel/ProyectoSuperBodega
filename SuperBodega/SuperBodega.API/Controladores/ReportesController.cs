using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.Infrastructure.Datos;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    public ReportesController(
        ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }


    [HttpGet("ventas-por-fecha")]
    public async Task<ActionResult> VentasPorFecha(
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var ventas = await _contexto.Ventas
            .Where(v =>
                v.Fecha >= fechaInicio &&
                v.Fecha <= fechaFin)
            .Include(v => v.Cliente)
            .ToListAsync();

        return Ok(ventas);
    }


    [HttpGet("ventas-por-producto")]
    public async Task<ActionResult> VentasPorProducto()
    {
        var reporte = await _contexto.DetalleVentas
            .Include(d => d.Producto)
            .GroupBy(d => d.Producto!.Nombre)
            .Select(g => new
            {
                Producto = g.Key,
                CantidadVendida =
                    g.Sum(x => x.Cantidad),

                TotalGenerado =
                    g.Sum(x =>
                        x.Cantidad *
                        x.PrecioUnitario)
            })
            .ToListAsync();

        return Ok(reporte);
    }


    [HttpGet("ventas-por-cliente")]
    public async Task<ActionResult> VentasPorCliente()
    {
        var reporte = await _contexto.Ventas
            .Include(v => v.Cliente)
            .GroupBy(v => v.Cliente!.Nombre)
            .Select(g => new
            {
                Cliente = g.Key,

                TotalCompras =
                    g.Count(),

                TotalGastado =
                    g.Sum(x => x.Total)
            })
            .ToListAsync();

        return Ok(reporte);
    }

    [HttpGet("ventas-por-proveedor")]
    public async Task<ActionResult> VentasPorProveedor()
    {
        var reporte = await _contexto.DetalleVentas
            .Include(d => d.Producto)
            .ThenInclude(p => p!.Proveedor)
            .GroupBy(d => d.Producto!.Proveedor!.Nombre)
            .Select(g => new
            {
                Proveedor = g.Key,

                ProductosVendidos =
                    g.Sum(x => x.Cantidad),

                TotalGenerado =
                    g.Sum(x =>
                        x.Cantidad *
                        x.PrecioUnitario)
            })
            .ToListAsync();

        return Ok(reporte);
    }
}