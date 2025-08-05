using borracharia.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração prioritária: ambiente > appsettings (apenas se não vazio)
builder.Configuration
    .AddEnvironmentVariables() // Prioridade máxima (Railway)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true); // Fallback

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // 1. Tenta pegar do Railway (formato postgres://)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // 2. Se não encontrou no ambiente, verifica appsettings (apenas se não vazio)
    if (string.IsNullOrEmpty(databaseUrl))
    {
        var configConnection = builder.Configuration.GetConnectionString("DefaultConnection");
        databaseUrl = !string.IsNullOrEmpty(configConnection) ? configConnection : null;
    }

    // 3. Conversão de formato se for URL do Railway
    if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgres://"))
    {
        databaseUrl = ConvertRailwayDbUrlToNpgsql(databaseUrl);
    }

    // 4. Validação final
    if (string.IsNullOrEmpty(databaseUrl))
    {
        throw new InvalidOperationException(
            "STRING DE CONEXÃO NÃO CONFIGURADA!\n" +
            "Defina no Railway: DATABASE_URL\n" +
            "OU no appsettings.json (apenas para desenvolvimento)");
    }

    options.UseNpgsql(databaseUrl);
});

// Método auxiliar para converter URL do Railway
static string ConvertRailwayDbUrlToNpgsql(string railwayUrl)
{
    var uri = new Uri(railwayUrl);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
           $"Username={userInfo[0]};Password={userInfo[1]};" +
           "SSL Mode=Require;Trust Server Certificate=true";
}

// Configurações padrão do app
builder.Services.AddControllersWithViews();

// Autenticação
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();

// Configurações de proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Sessão
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Middleware pipeline
app.UseForwardedHeaders();
app.Use((context, next) =>
{
    if (context.Request.Headers["X-Forwarded-Proto"] == "https")
    {
        context.Request.Scheme = "https";
    }
    return next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

    // Aplica migrações automaticamente em produção
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();