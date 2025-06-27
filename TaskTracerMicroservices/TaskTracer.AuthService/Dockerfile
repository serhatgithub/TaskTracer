# Aşama 1: Build (SDK imajını kullanarak uygulamayı derle)
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build-env
WORKDIR /app

# Proje dosyasını kopyala ve bağımlılıkları yükle (NuGet restore)
# Sadece .csproj değiştiğinde bu katman yeniden çalışır, daha hızlı build için.
COPY *.csproj ./
RUN dotnet restore

# Tüm kaynak kodunu kopyala
COPY . ./

# Uygulamayı publish et (Release modunda, optimize edilmiş çıktılar için)
RUN dotnet publish -c Release -o out

# Aşama 2: Runtime (ASP.NET Core runtime imajını kullanarak uygulamayı çalıştır)
# Bu imaj SDK imajından daha küçüktür, çünkü sadece çalıştırma için gerekenleri içerir.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app

# Build aşamasından publish edilen çıktıları kopyala
COPY --from=build-env /app/out .

# Uygulamanın dinleyeceği port (genellikle 80 veya 8080)
# Bu, Kestrel'in varsayılan olarak dinlediği porttur. Dışarıya map etmeyi Docker Compose'da yapacağız.
EXPOSE 80
EXPOSE 443 

# Uygulamayı başlatma komutu
# Veritabanı dosyası için bir volume mount veya başlatma script'i ile migration'ların çalıştırılması düşünülebilir.
# Program.cs'teki otomatik migration kodu burada işe yarayacak.
ENTRYPOINT ["dotnet", "TaskTracer.AuthService.dll"]