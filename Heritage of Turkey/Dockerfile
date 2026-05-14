# 1. Aşama: Uygulamayı çalıştırmak için hafif bir temel imaj (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Aşama: Kodları derlemek için SDK imajı (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sadece csproj dosyasını kopyalayıp bağımlılıkları yüklüyoruz (Cache avantajı için)
COPY ["Heritage of Turkey.csproj", "./"]
RUN dotnet restore "Heritage of Turkey.csproj"

# Projenin geri kalan tüm dosyalarını kopyalayıp Release modunda derliyoruz
COPY . .
RUN dotnet build "Heritage of Turkey.csproj" -c Release -o /app/build

# 3. Aşama: Yayınlama (Publish)
FROM build AS publish
RUN dotnet publish "Heritage of Turkey.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Aşama: Çalıştırma (Final)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render ayağa kalkarken bu komutu çalıştıracak
ENTRYPOINT ["dotnet", "Heritage of Turkey.dll"]