using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBodega.Web.Modelos;

namespace SuperBodega.Web.Controllers;

public class ProductosController : Controller
{
    private readonly HttpClient _httpClient;

    public ProductosController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        var respuesta = await _httpClient.GetAsync("api/productos");

    
        if (!respuesta.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "No se pudieron cargar los productos desde el servidor.");
            return View(new List<Producto>());
        }

        var json = await respuesta.Content.ReadAsStringAsync();

        var productos = JsonConvert.DeserializeObject<List<Producto>>(json);

        return View(productos);
    }
}