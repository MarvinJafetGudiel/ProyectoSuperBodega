using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SuperBodega.Web.Modelos;

namespace SuperBodega.Web.Controllers;

public class DashboardController : Controller
{
    private readonly HttpClient _httpClient;

    public DashboardController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var respuesta = await _httpClient.GetAsync("api/reportes/ventas-por-producto");

            if (!respuesta.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "No se pudo cargar el reporte desde el servidor.");
                return View(new List<ReporteProducto>());
            }

            var json = await respuesta.Content.ReadAsStringAsync();

            var reporte = JsonConvert.DeserializeObject<List<ReporteProducto>>(json);

            return View(reporte);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error de conexión: {ex.Message}");
            return View(new List<ReporteProducto>());
        }
    }
}