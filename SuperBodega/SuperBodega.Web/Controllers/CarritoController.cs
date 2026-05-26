using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBodega.Web.Modelos;
using System.Text;
using System.Net.Http.Headers;

namespace SuperBodega.Web.Controllers;

public class CarritoController : Controller
{
    private static List<CarritoItem> carrito = new();
    private readonly HttpClient _httpClient;

    public CarritoController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public IActionResult Index()
    {
        return View(carrito);
    }

    public IActionResult Agregar(int productoId, string nombre, decimal precio)
    {
        var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);

        if (item == null)
        {
            carrito.Add(new CarritoItem
            {
                ProductoId = productoId,
                Nombre = nombre,
                Precio = precio,
                Cantidad = 1
            });
        }
        else
        {
            item.Cantidad++;
        }

        return RedirectToAction("Index", "Productos");
    }

    public IActionResult Eliminar(int productoId)
    {
        var item = carrito.FirstOrDefault(x => x.ProductoId == productoId);

        if (item != null)
        {
            carrito.Remove(item);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> FinalizarCompra()
    {

        var venta = new VentaDTO
        {
            ClienteId = 1,
            Productos = new List<ProductoVentaDTO>() 
        };

        foreach (var item in carrito)
        {
            venta.Productos.Add(new ProductoVentaDTO
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad
            });
        }

        var json = JsonConvert.SerializeObject(venta);
        var contenido = new StringContent(json, Encoding.UTF8, "application/json");


        var respuesta = await _httpClient.PostAsync("api/ventas", contenido);

        if (respuesta.IsSuccessStatusCode)
        {
            carrito.Clear();
            TempData["mensaje"] = "Compra realizada correctamente";
            return RedirectToAction("Index");
        }

        TempData["mensaje"] = "Error al realizar la compra en el servidor.";
        return RedirectToAction("Index");
    }
}