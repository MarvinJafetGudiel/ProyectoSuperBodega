using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.DTOs;
using SuperBodega.Domain.Entidades;
using SuperBodega.Infrastructure.Datos;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    public ClientesController(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> ObtenerClientes()
    {
        var clientes = await _contexto.Clientes.ToListAsync();

        return Ok(clientes);
    }

    [HttpPost]
    public async Task<ActionResult> CrearCliente(ClienteDTO dto)
    {
        var cliente = new Cliente
        {
            Nombre = dto.Nombre,
            Correo = dto.Correo,
            Telefono = dto.Telefono
        };

        _contexto.Clientes.Add(cliente);

        await _contexto.SaveChangesAsync();

        return Ok(cliente);
    }
}