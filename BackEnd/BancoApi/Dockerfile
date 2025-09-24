# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyectos
COPY Banco.Application/Banco.Application.csproj Banco.Application/
COPY Banco.Domain/Banco.Domain.csproj Banco.Domain/
COPY Banco.Infrastructure/Banco.Infrastructure.csproj Banco.Infrastructure/
COPY BancoApi/BancoApi.csproj BancoApi/

# Restaurar dependencias
RUN dotnet restore BancoApi/BancoApi.csproj

# Copiar todo el código
COPY . .

# Publicar en Release
RUN dotnet publish BancoApi/BancoApi.csproj -c Release -o /app/publish

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BancoApi.dll"]
