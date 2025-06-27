using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ocelot.json dosyasını yapılandırmaya ekle
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Ocelot servislerini DI container'ına ekle
builder.Services.AddOcelot(builder.Configuration);

// Swagger servisini ekle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Frontend'den API Gateway'e erişim için CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Swagger middleware'ini ekle (geliştirme ortamında)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS middleware'ini kullan
app.UseCors("AllowAll");

// Ocelot middleware'ini HTTP request pipeline'ına ekle
await app.UseOcelot();

app.Run();
