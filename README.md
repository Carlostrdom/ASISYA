# ASISYA — Prueba Técnica

API REST para gestión de productos y categorías, desarrollada con .NET 10 y Angular 21 bajo principios de arquitectura limpia.

---

## Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| Backend | .NET 10, ASP.NET Core |
| Base de datos | PostgreSQL 16 |
| ORM | Entity Framework Core 9 + Npgsql |
| Autenticación | JWT Bearer (HS256, 60 min) |
| Hashing | BCrypt.Net |
| Cache | IMemoryCache (in-process) |
| Tests | xUnit, Moq, WebApplicationFactory |
| Contenedores | Docker, Docker Compose |
| CI/CD | GitHub Actions |
| Frontend | Angular 21 (SPA) |

---

## Arquitectura

```
Asisya.Domain          → Entidades y contratos (interfaces de repositorios)
Asisya.Application     → DTOs, interfaces de servicios, lógica de negocio
Asisya.Infrastructure  → EF Core, repositorios, JWT, BCrypt, DbContext
Asisya.API             → Controllers, middleware, configuración DI
Asisya.Tests           → Unit tests (Moq) + Integration tests (WebApplicationFactory)
asisya-frontend/       → Angular 21 SPA
```

### Decisiones arquitectónicas

**¿Por qué arquitectura en capas y no Hexagonal pura?**
La prueba requiere CRUD rápido con exposición de endpoints claros. La arquitectura en capas ofrece la misma separación de responsabilidades con menos overhead de puertos/adaptadores explícitos, manteniendo la testabilidad y la inversión de dependencias.

**¿Por qué mapeo manual y no AutoMapper?**
El mapeo manual hace visibles las transformaciones, evita errores silenciosos de convención y elimina dependencias externas innecesarias.

**¿Por qué IMemoryCache y no Redis?**
Para una sola instancia, `IMemoryCache` es suficiente sin infraestructura adicional. En producción multi-instancia se reemplaza por Redis (ver sección de escalabilidad).

**¿Por qué batch inserts de 1000?**
EF Core con `AddRangeAsync` + `SaveChangesAsync` en bloques de 1.000 registros y `ChangeTracker.Clear()` entre bloques evita saturar el change tracker y mantiene el consumo de memoria constante.

---

## Opción A — Docker (recomendado)

### Requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Pasos

```bash
git clone https://github.com/Carlostrdom/ASISYA.git
cd ASISYA
docker-compose up --build
```

Esto levanta automáticamente los 3 servicios:

| Servicio | Imagen | Puerto |
|----------|--------|--------|
| PostgreSQL 16 | `postgres:16-alpine` | `5432` |
| API .NET 10 | build local | `5200` |
| Frontend Angular | build local (Nginx) | `4200` |

Las tablas se crean automáticamente en la primera ejecución.

**Accesos:**
- Frontend: `http://localhost:4200`
- API: `http://localhost:5200`
- Swagger UI: `http://localhost:5200/swagger`

**Detener (conserva los datos):**
```bash
docker-compose down
```

**Detener y limpiar la base de datos:**
```bash
docker-compose down -v
```

---

## Opción B — Ejecución local (sin Docker)

### Requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Node.js 22+](https://nodejs.org/)

### 1. Clonar el repositorio

```bash
git clone https://github.com/Carlostrdom/ASISYA.git
cd ASISYA
```

### 2. Configurar la cadena de conexión

Edita `Asisya.API/appsettings.json` con tus credenciales de PostgreSQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=asisya_db;Username=postgres;Password=TU_PASSWORD"
}
```

### 3. Ejecutar la API

```bash
dotnet run --project Asisya.API/Asisya.API.csproj --launch-profile http
```

Las tablas se crean automáticamente en la primera ejecución (EF Core Migrate).

Disponible en:
- API: `http://localhost:5200`
- Swagger UI: `http://localhost:5200/swagger`

### 4. Ejecutar el Frontend

En otra terminal:

```bash
cd asisya-frontend
npm install
npm start
```

Disponible en `http://localhost:4200`

---

## Endpoints de la API

### Autenticación (público)

```
POST /api/auth/register   → Registrar usuario
POST /api/auth/login      → Obtener token JWT
```

### Categorías (requiere JWT)

```
POST   /api/category        → Crear categoría
GET    /api/category        → Listar todas las categorías
GET    /api/category/{id}   → Detalle de categoría
```

### Productos (requiere JWT)

```
GET    /api/product                                                       → Listar con paginación
GET    /api/product?page=1&pageSize=20&search=laptop&categoryId=1         → Con filtros
GET    /api/product/{id}    → Detalle con datos de categoría y proveedor
POST   /api/product         → Crear producto individual
POST   /api/product/bulk    → Generar N productos aleatorios (carga masiva)
PUT    /api/product/{id}    → Actualizar producto
DELETE /api/product/{id}    → Eliminar producto
```

**Carga masiva:**
```json
POST /api/product/bulk
{
  "count": 100000,
  "categoryID": 1
}
```

---

## Flujo de prueba recomendado

1. `POST /api/auth/register` → crear usuario
2. `POST /api/auth/login` → obtener token JWT
3. En Swagger: botón **Authorize** → `Bearer {token}`
4. `POST /api/category` → crear categoría (ej. `"categoryName": "Servidores"`)
5. `POST /api/product/bulk` con `{ "count": 100000, "categoryID": 1 }` → cargar 100k productos
6. `GET /api/product?page=1&pageSize=20` → verificar paginación y cache

---

## Tests

```bash
# Unit tests (sin base de datos)
dotnet test Asisya.Tests/Asisya.Tests.csproj --filter "FullyQualifiedName~Unit"

# Integration tests (requiere PostgreSQL activo con la connection string configurada)
dotnet test Asisya.Tests/Asisya.Tests.csproj --filter "FullyQualifiedName~Integration"

# Todos los tests
dotnet test Asisya.Tests/Asisya.Tests.csproj
```

**Cobertura:** 21 tests — 17 unitarios (Moq) + 4 de integración (WebApplicationFactory + PostgreSQL real).

---

## CI/CD

El pipeline de GitHub Actions (`.github/workflows/ci.yml`) ejecuta en cada push/PR a `main`:

| Job | Descripción |
|-----|-------------|
| **Build & Unit Tests** | Compila en Release y corre los 17 unit tests (sin BD) |
| **Integration Tests** | PostgreSQL como service container + 4 tests end-to-end |
| **Docker Build** | Construye las imágenes de API y Frontend (solo si los tests pasan) |

---

## Escalabilidad horizontal

La API es **stateless** (JWT sin sesiones en servidor), lo que permite escalar horizontalmente sin coordinación.

```
Internet
    │
    ▼
Load Balancer (ALB / Azure Front Door / nginx)
    │         │         │
    ▼         ▼         ▼
 API Pod   API Pod   API Pod     ← ECS Fargate / Kubernetes con HPA
    │
    ├── Redis (reemplaza IMemoryCache en multi-instancia)
    │
    └── PostgreSQL (RDS / Cloud SQL)
            └── Read Replica  ← lecturas GET se redirigen aquí
```

| Componente | Acción para escalar |
|------------|---------------------|
| **API** | Contenedores en ECS Fargate / GKE, auto-scaling por CPU > 70% |
| **Cache** | Migrar `IMemoryCache` → `IDistributedCache` con Redis |
| **Base de datos** | Read replica para separar lecturas de escrituras |
| **Bulk insert** | Para cargas > 500k, colas (SQS / Azure Service Bus) con workers |
| **Migraciones** | Job de Kubernetes previo al despliegue, no al arrancar el pod |
| **Observabilidad** | OpenTelemetry → CloudWatch / Datadog |
