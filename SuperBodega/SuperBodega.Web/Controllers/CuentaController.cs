using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SuperBodega.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SuperBodega.Web.Controllers;

[Authorize]

public class CuentaController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (
            modelo.Usuario == "admin" &&
            modelo.Password == "1234"
        )
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, modelo.Usuario),
                new Claim(ClaimTypes.Role, "Administrador")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction(
                "Index",
                "Home");
        }

        ViewBag.Error = "Usuario o contraseña incorrectos";

        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            "Login");
    }
}