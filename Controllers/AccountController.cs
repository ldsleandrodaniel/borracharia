using borracharia.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace borracharia.Controllers
{
    public class AccountController : Controller
    {
        private readonly Usuario _usuario;

        public AccountController(IConfiguration config)
        {
            // 1. Prioriza variáveis de ambiente (Railway)
            var envUser = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
            var envPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            // 2. Fallback para appsettings.json apenas se não vazio
            var configUser = config["AdminCredentials:Username"];
            var configPass = config["AdminCredentials:Password"];

            _usuario = new Usuario
            {
                NomeUsuario = !string.IsNullOrEmpty(envUser) ? envUser
                             : (!string.IsNullOrEmpty(configUser) ? configUser : null),

                Senha = !string.IsNullOrEmpty(envPass) ? envPass
                        : (!string.IsNullOrEmpty(configPass) ? configPass : null)
            };

            // Validação rigorosa
            if (string.IsNullOrEmpty(_usuario.NomeUsuario) || string.IsNullOrEmpty(_usuario.Senha))
            {
                throw new InvalidOperationException(
                    "CREDENCIAIS NÃO CONFIGURADAS!\n" +
                    "Defina no Railway:\n" +
                    "ADMIN_USERNAME e ADMIN_PASSWORD\n" +
                    "OU no appsettings.json (apenas para desenvolvimento)");
            }

            // Log para debug (remova em produção)
            Console.WriteLine($"Usuário admin carregado: {_usuario.NomeUsuario}");
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

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = false // Cookie não persistente
                        });

                    return Url.IsLocalUrl(returnUrl)
                        ? Redirect(returnUrl)
                        : RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Credenciais inválidas");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}