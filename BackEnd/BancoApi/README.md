# Levantar Backend con Docker y EF Core

## Levantar contenedores

Abre una terminal en la raíz del proyecto y ejecuta:

```bash
docker-compose up --build -d
```

```bash
docker ps

```

Ejecutar migraciones

Con .NET SDK instalado localmente, ejecuta:
```bash
dotnet tool install --global dotnet-ef
```

```bash
dotnet ef database update --project Banco.Infrastructure\Banco.Infrastructure.csproj --startup-project BancoApi\BancoApi.csproj

```

--project → Proyecto donde están tus migraciones (Banco.Infrastructure).

--startup-project → Proyecto que tiene la configuración de DbContext (BancoApi).

Acceder al backend

Una vez levantado y migrado, abre en tu navegador:

http://localhost:5000/swagger
