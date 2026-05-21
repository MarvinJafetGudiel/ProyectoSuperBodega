using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBodega.Web.Modelos;

namespace SuperBodega.Web.Controllers;

public class ProductosController : Controller
{
    private readonly HttpClient _httpClient;

    public ProductosController()
    {
        _httpClient = new HttpClient();

        _httpClient.BaseAddress =
            new Uri("https://localhost:7230/");
    }

    public async Task<IActionResult> Index()
    {
        var respuesta =
            await _httpClient.GetAsync("api/productos");

        var json = await respuesta.Content.ReadAsStringAsync();

        var productos =
            JsonConvert.DeserializeObject<List<Producto>>(json);

        return View(productos);
    }
}