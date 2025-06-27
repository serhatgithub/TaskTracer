# Aşama 1: Build (SDK imajını kullanarak uygulamayı derle)
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build-env
WORKDIR /app

# Proje dosyasını kopyala ve bağımlılıkları yükle
COPY *.csproj ./
RUN dotnet restore

# Tüm kaynak kodunu ve ocelot.json'ı kopyala
COPY . ./

# Uygulamayı publish et
RUN dotnet publish -c Release -o out

# Aşama 2: Runtime (ASP.NET Core runtime imajını kullanarak uygulamayı çalıştır)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app

# Build aşamasından publish edilen çıktıları kopyala
COPY --from=build-env /app/out .

# Uygulamanın dinleyeceği port (ocelot.json'daki BaseUrl'den gelir ama Kestrel varsayılan olarak 80/443'ü dinler)
EXPOSE 80
EXPOSE 443

# Uygulamayı başlatma komutu
ENTRYPOINT ["dotnet", "TaskTracer.ApiGateway.dll"]