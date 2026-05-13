using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.DTOs;
using SuperBodega.Domain.Entidades;
using SuperBodega.Infrastructure.Datos;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    public ProveedoresController(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Proveedor>>> ObtenerProveedores()
    {
        var proveedores = await _contexto.Proveedores.ToListAsync();

        return Ok(proveedores);
    }

    [HttpPost]
    public async Task<ActionResult> CrearProveedor(ProveedorDTO dto)
    {
        var proveedor = new Proveedor
        {
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            Telefono = dto.Telefono
        };

        _contexto.Proveedores.Add(proveedor);

        await _contexto.SaveChangesAsync();

        return Ok(proveedor);
    }
}