
using AdminBareSkin.Data;
using AdminBareSkin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CONEXIÓN CON SUPABASE POSTGRESQL
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// AUTENTICACIÓN Y USUARIOS
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// SERVICIOS FINANCIEROS
builder.Services.AddScoped<CierreMensualService>();

// SERVICIO DE CIERRE AUTOMÁTICO
builder.Services.AddHostedService<CierreAutomaticoService>();

// CONTROLADORES Y VISTAS
builder.Services.AddControllersWithViews();

var app = builder.Build();

// CREAR AUTOMÁTICAMENTE LOS TRES USUARIOS
await IdentitySeeder.SeedAsync(
    app.Services,
    app.Configuration
);

// CONFIGURACIÓN DE LA APLICACIÓN
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
