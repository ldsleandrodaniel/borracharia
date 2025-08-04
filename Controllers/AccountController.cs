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

            // Novo sistema híbrido (Environment Variables + appsettings)
            _usuario = new Usuario
            {
                NomeUsuario = Environment.GetEnvironmentVariable("ADMIN_USERNAME")
                             ?? _config["AdminCredentials:Username"],

                Senha = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                       ?? _config["AdminCredentials:Password"]
            };

            // Validação crítica (Obrigatória para produção)
            if (string.IsNullOrEmpty(_usuario.NomeUsuario) || string.IsNullOrEmpty(_usuario.Senha))
            {
                throw new InvalidOperationException(
                    "Credenciais de administrador não configuradas!\n" +
                    "Defina as variáveis de ambiente ADMIN_USERNAME e ADMIN_PASSWORD\n" +
                    "OU configure a seção AdminCredentials no appsettings.json");
            }
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

