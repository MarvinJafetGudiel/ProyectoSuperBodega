# SuperBodega

**Sistema Inteligente de Gestión de Supermercado**

Aplicación web completa desarrollada con arquitectura limpia en .NET 10, que permite gestionar productos, clientes, proveedores, compras, ventas y reportes administrativos en tiempo real.

---

## Presentación del Proyecto

[`SuperBodega_Presentacion.pptx`](./SuperBodega_Presentacion.pptx) — Presentación completa de 15 diapositivas con capturas del sistema, arquitectura, pruebas de rendimiento y despliegue en la nube.

---

## Tecnologías

| Capa | Tecnología |
|------|-----------|
| Backend | .NET 10 / ASP.NET Core / C# |
| Base de datos | PostgreSQL + Entity Framework Core |
| Mensajería | RabbitMQ (patrón Productor/Consumidor) |
| Frontend | ASP.NET Core MVC + Razor Views |
| Contenedores | Docker + docker-compose |
| Nube | Railway |
| Pruebas de carga | K6 |
| Documentación API | Swagger / OpenAPI 3.0 |

---

## Arquitectura

El proyecto sigue el patrón de **Arquitectura Limpia** organizado en 4 capas:

```
SuperBodega.Domain         → Entidades del negocio (Producto, Venta, Cliente…)
SuperBodega.Infrastructure → EF Core + PostgreSQL + Migrations
SuperBodega.API            → Controllers REST + RabbitMQ + Swagger
SuperBodega.Web            → Frontend MVC (Carrito, Dashboard, Login)
```

---

## Módulos de la API

| Módulo | Endpoints |
|--------|-----------|
| Categorías | GET, POST |
| Clientes | GET, POST |
| Compras | GET, POST |
| Productos | GET, POST, PUT, DELETE |
| Proveedores | GET, POST |
| Ventas | GET, POST |
| Reportes | ventas-por-fecha, ventas-por-producto, ventas-por-cliente, ventas-por-proveedor |

Documentación disponible en:
- **Producción (Railway):** [https://proyectosuperbodega-production.up.railway.app/swagger/index.html](https://proyectosuperbodega-production.up.railway.app/swagger/index.html)
- **Local (Docker):** `http://localhost:8080/swagger`

---

## Levantar con Docker

```bash
git clone https://github.com/MarvinJafetGudiel/ProyectoSuperBodega.git
cd ProyectoSuperBodega/SuperBodega
docker-compose up
```

Servicios disponibles tras ejecutar:

| Servicio | URL |
|----------|-----|
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| RabbitMQ panel | http://localhost:15672 |
| PostgreSQL | localhost:5432 |

---

## Flujo de Mensajería Asincrona

```
Venta creada → RabbitMQProductor → Cola "ventas_durable" → RabbitMQConsumidor → Email de confirmación
```

Al finalizar una compra, la API publica un mensaje en RabbitMQ. El consumidor lo procesa de forma asincrona y envía un email de confirmación al cliente, sin bloquear la respuesta principal.

---

## Pruebas de Rendimiento (K6)

Ejecutadas con 5 usuarios virtuales (VUs) y 25 iteraciones:

| Métrica | Prueba Síncrona | Prueba Asincrona |
|---------|----------------|-----------------|
| Avg. Response | 58.94 ms | 35.72 ms |
| Min. Response | 29.21 ms | 13.59 ms |
| Max. Response | 93.67 ms | 95.99 ms |
| p(90) | 82.1 ms | 56.62 ms |
| p(95) | 91.11 ms | 62.08 ms |
| Checks pasados | **100 %** | **100 %** |

---

## Despliegue en Railway

El sistema está desplegado en producción con los tres servicios en estado **Online**:
- `ProyectoSuperBodega` (API .NET)
- `Postgres` (con volumen persistente)
- `RabbitMQ` (con volumen persistente)

La API detecta automáticamente el entorno (Local / Docker / Railway) y ajusta la configuración sin cambios de código.

**API en producción:** [https://proyectosuperbodega-production.up.railway.app/swagger/index.html](https://proyectosuperbodega-production.up.railway.app/swagger/index.html)
