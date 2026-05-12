using Microsoft.EntityFrameworkCore;
using SuperBodega.Domain.Entidades;

namespace SuperBodega.Infrastructure.Datos;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<Categoria> Categorias => Set<Categoria>();

	public DbSet<Producto> Productos => Set<Producto>();

	public DbSet<Cliente> Clientes => Set<Cliente>();

	public DbSet<Proveedor> Proveedores => Set<Proveedor>();

	public DbSet<Venta> Ventas => Set<Venta>();

	public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
}