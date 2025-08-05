using borracharia.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configura��o priorit�ria: ambiente > appsettings (apenas se n�o vazio)
builder.Configuration
    .AddEnvironmentVariables() // Prioridade m�xima (Railway)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true); // Fallback

// Configura��o do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // 1. Tenta pegar do Railway (formato postgres://)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // 2. Se n�o encontrou no ambiente, verifica appsettings (apenas se n�o vazio)
    if (string.IsNullOrEmpty(databaseUrl))
    {
        var configConnection = builder.Configuration.GetConnectionString("DefaultConnection");
        databaseUrl = !string.IsNullOrEmpty(configConnection) ? configConnection : null;
    }

    // 3. Convers�o de formato se for URL do Railway
    if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("postgres://"))
    {
        databaseUrl = ConvertRailwayDbUrlToNpgsql(databaseUrl);
    }

    // 4. Valida��o final
    if (string.IsNullOrEmpty(databaseUrl))
    {
        throw new InvalidOperationException(
            "STRING DE CONEX�O N�O CONFIGURADA!\n" +
            "Defina no Railway: DATABASE_URL\n" +
            "OU no appsettings.json (apenas para desenvolvimento)");
    }

    options.UseNpgsql(databaseUrl);
});

// M�todo auxiliar para converter URL do Railway
static string ConvertRailwayDbUrlToNpgsql(string railwayUrl)
{
    var uri = new Uri(railwayUrl);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
           $"Username={userInfo[0]};Password={userInfo[1]};" +
           "SSL Mode=Require;Trust Server Certificate=true";
}

// Configura��es padr�o do app
builder.Services.AddControllersWithViews();

// Autentica��o
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

// Configura��es de proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Sess�o
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

    // Aplica migra��es automaticamente em produ��o
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