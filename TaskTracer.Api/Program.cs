using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskTracer.Api.Data;
using TaskTracer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// JWT servisleri
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddCors();


builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// DB bağlantısı
builder.Services.AddDbContext<TaskTrackerContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });


// Swagger (manual Swashbuckle kurulumu)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskTracer API", Version = "v1" });

    // JWT Auth için Swagger'a bearer token header'ı ekleyelim
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
    };
    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityReq = new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            new string[] {}
        }
    };
    c.AddSecurityRequirement(securityReq);
});

var app = builder.Build();

// Swagger sadece geliştirmede aktif
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());


app.MapControllers();

app.Run();
