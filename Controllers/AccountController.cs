using borracharia.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace borracharia.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly Usuario _usuario;

        public AccountController(IConfiguration config)
        {
            _config = config;
            _usuario = new Usuario
            {
                NomeUsuario = _config["AdminCredentials:Username"],
                Senha = _config["AdminCredentials:Password"]
            };
        }


        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Usuario model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (model.NomeUsuario == _usuario.NomeUsuario && model.Senha == _usuario.Senha)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, _usuario.NomeUsuario),
                    new Claim(ClaimTypes.Role, "Administrador")
                };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

