# Aşama 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build-env
WORKDIR /app

# Sadece .csproj dosyasını kopyala ve bağımlılıkları yükle
COPY *.csproj ./
RUN dotnet restore

# Tüm kaynak kodunu kopyala
COPY . ./

# Uygulamayı publish et
RUN dotnet publish -c Release -o out

# Aşama 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app
COPY --from=build-env /app/out .

# Her proje için bu son satırı kendi .dll adıyla güncelle!
ENTRYPOINT ["dotnet", "TaskTracer.TaskService.dll"]