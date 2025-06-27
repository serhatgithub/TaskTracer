using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskTracer.AuthService.Data;
using TaskTracer.AuthService.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// 1. Servisleri Kaydetme
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PasswordService>();

// 2. Veritabanı Bağlantısını Kaydetme (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("AuthDbConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Controller'ları ekleme
builder.Services.AddControllers();

// 4. CORS Politikası (Geliştirme için şimdilik her şeye izin verelim)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Swagger/OpenAPI Entegrasyonu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskTracer Authentication API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Ortama göre Swagger ve Hata sayfası yapılandırması
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskTracer Auth API V1");
        c.RoutePrefix = string.Empty;
    });

    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");

// JWT Auth eğer yapılandırıldıysa burada açılmalı
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Otomatik migration için bir helper fonksiyonu
void ApplyMigrations(IApplicationBuilder app)
{
    using (var scope = app.ApplicationServices.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Veritabanı migration'ları uygulanıyor...");
            var context = services.GetRequiredService<AuthDbContext>();
            context.Database.Migrate();
            logger.LogInformation("Veritabanı migration'ları başarıyla uygulandı.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabanı migration işlemi sırasında kritik bir hata oluştu.");
        }
    }
}

// Migration'ları uygula
ApplyMigrations(app);

app.Run();
