using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.DTOs;
using SuperBodega.Domain.Entidades;
using SuperBodega.Infrastructure.Datos;

namespace SuperBodega.API.Controladores;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ApplicationDbContext _contexto;

    public ProductosController(ApplicationDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> ObtenerProductos()
    {
        var productos = await _contexto.Productos
            .Include(p => p.Categoria)
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> ObtenerProducto(int id)
    {
        var producto = await _contexto.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null)
        {
            return NotFound("Producto no encontrado");
        }

        return Ok(producto);
    }


    [HttpPost]
    public async Task<ActionResult> CrearProducto(ProductoDTO dto)
    {
        var categoriaExiste = await _contexto.Categorias
            .AnyAsync(c => c.Id == dto.CategoriaId);

        if (!categoriaExiste)
        {
            return BadRequest("La categoría no existe");
        }

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            CategoriaId = dto.CategoriaId
        };

        _contexto.Productos.Add(producto);

        await _contexto.SaveChangesAsync();

        return Ok(producto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> ActualizarProducto(int id, ProductoDTO dto)
    {
        var producto = await _contexto.Productos.FindAsync(id);

        if (producto == null)
        {
            return NotFound("Producto no encontrado");
        }

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.CategoriaId = dto.CategoriaId;

        await _contexto.SaveChangesAsync();

        return Ok(producto);
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarProducto(int id)
    {
        var producto = await _contexto.Productos.FindAsync(id);

        if (producto == null)
        {
            return NotFound("Producto no encontrado");
        }

        _contexto.Productos.Remove(producto);

        await _contexto.SaveChangesAsync();

        return Ok("Producto eliminado");
    }
}