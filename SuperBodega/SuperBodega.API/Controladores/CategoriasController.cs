using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.Domain.Entidades;
using SuperBodega.Infrastructure.Datos;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    public CategoriasController(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> ObtenerCategorias()
    {
        var categorias = await _contexto.Categorias.ToListAsync();

        return Ok(categorias);
    }

    [HttpPost]
    public async Task<ActionResult> CrearCategoria(Categoria categoria)
    {
        _contexto.Categorias.Add(categoria);

        await _contexto.SaveChangesAsync();

        return Ok(categoria);
    }
}