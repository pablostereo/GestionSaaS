using System.Text;
using System.Text.Json.Serialization;
using GestionSaaS.Api.Infraestructura;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Negocio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Api ---
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddExceptionHandler<ManejoErrores>();
builder.Services.AddProblemDetails();

// --- Swagger con botón "Authorize" para pegar el token ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Hacer login en /api/auth/login y pegar acá el token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
    });
});

// --- Autenticación JWT ---
var jwt = builder.Configuration.GetSection("Jwt").Get<ConfiguracionJwt>() ?? new ConfiguracionJwt();
if (jwt.Clave.Length < 32)
    throw new InvalidOperationException("Falta configurar Jwt:Clave (mínimo 32 caracteres).");

builder.Services.Configure<ConfiguracionJwt>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<GeneradorToken>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;   // conserva los nombres de claims tal cual ("role", "cliente_id")
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Emisor,
            ValidAudience = jwt.Emisor,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            RoleClaimType = "role",
            NameClaimType = "name",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Por defecto todo endpoint requiere estar logueado (salvo [AllowAnonymous])
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// --- Cliente actual (multi-tenant, desde el token) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClienteActual, ClienteActualHttp>();

// --- Negocio + Datos (proveedor elegido en appsettings.json → "BaseDatos:Proveedor") ---
var proveedor = builder.Configuration.GetValue("BaseDatos:Proveedor", ProveedorBD.Sqlite);
builder.Services.AddNegocio(
    proveedor,
    builder.Configuration.GetConnectionString(proveedor.ToString()),
    builder.Environment.ContentRootPath);

var app = builder.Build();

// Crea la base y carga admin + cliente demo
await InicializadorDatos.InicializarAsync(app.Services);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
