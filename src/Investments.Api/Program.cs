using System.Text;
using System.Text.Json.Serialization;
using Investments.Api.Middlewares;
using Investments.Infrastructure;
using Investments.Infrastructure.Data;
using Investments.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Camadas (DI)
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticação JWT
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? throw new InvalidOperationException("Seção 'Jwt' não configurada.");
if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
    throw new InvalidOperationException("Jwt:Secret deve ter ao menos 32 caracteres.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // mantém "sub" como nome da claim
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// Controllers + serialização
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Documentação (Swagger) com suporte a Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Investments API",
        Version = "v1",
        Description = "API de investimentos - Atividade Substitutiva Fase 1 (POSTECH)"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido em POST /auth."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    var xml = Path.Combine(AppContext.BaseDirectory, "Investments.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

var app = builder.Build();

// Aplica migrations automaticamente (somente quando for SQL Server)
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlServer())
        db.Database.Migrate();
}

// O log fica por fora para registrar tambem as respostas de erro ja tratadas (400/401/404/409).
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Investments API v1"));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Permite que os testes de integração referenciem a classe Program.
public partial class Program { }
