using ManejoTareas;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using ManejoTareas.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var politicaUsuarioAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                                                  .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(opc => {
    opc.Filters.Add(new AuthorizeFilter(politicaUsuarioAutenticados));
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
  .AddDataAnnotationsLocalization(opc => {
      opc.DataAnnotationLocalizerProvider = (_, factoria) => 
        factoria.Create(typeof(RecursoCompartido)
      );
  })
  .AddJsonOptions(opc => {
      opc.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  });

/* Configurando el DbContext */
builder.Services.AddDbContext<ApplicationDbContext>(opc => opc.UseSqlServer("name=DefaultConnection"));

builder.Services.AddAuthentication().AddMicrosoftAccount(opc => {
    opc.ClientId = builder.Configuration["MicrosoftClientId"];
    opc.ClientSecret = builder.Configuration["MicrosoftSecretId"];
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opc => { 
    opc.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

/* Evitamos las vistas por defecto */
builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, opc => {
    opc.LoginPath = "/Usuarios/Login";
    opc.AccessDeniedPath = "/Usuarios/Login";
});

/* Para idiomas - localizacion */
builder.Services.AddLocalization(opc => {
    opc.ResourcesPath = "Resources";
});

builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IAlmacenadorArchivosRepository, AlmacenadorArchivosRepository>();

var app = builder.Build();

app.UseRequestLocalization(opc => {
    /* Cultura por defecto */
    opc.DefaultRequestCulture = new RequestCulture("es");

    /* Culturas soportadas */
    opc.SupportedUICultures = Constantes.culturasSoportadas.Select(cul => new CultureInfo(cul.Value))
                                      .ToList();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
