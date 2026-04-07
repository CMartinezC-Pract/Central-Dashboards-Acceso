# DataXcaret — Portal de Business Intelligence

> Portal web interno para centralizar el acceso a dashboards de Power BI, gestionar incidencias y solicitudes de cambio en las Unidades de Negocio de Grupo Xcaret.

---

## Descripción

**DataXcaret** es una aplicación web corporativa desarrollada sobre **ASP.NET Core 8 con Razor Pages**, desplegada en la red interna de Grupo Xcaret. Concentra en un solo lugar todos los dashboards de Power BI de las distintas Unidades de Negocio (Parques y Tours, Hoteles, Xailing, MDC, Entorno Turístico, Talento Humano, entre otras), con un sistema de permisos por usuario, seguimiento formal de incidencias y solicitudes, y un panel administrativo con auditoría completa.

---

## Características principales

- 📊 **Catálogo de dashboards** organizado por Unidad de Negocio, con clasificación por nivel (Operativo / Táctico / Estratégico)
- 🔒 **Autenticación y permisos** por usuario — roles Administrador y Usuario
- 🎫 **Gestión de tickets** — Incidencias (`INC-AAAA-NNN`) y Solicitudes (`SOL-AAAA-NNN`) con folio único, comentarios y seguimiento de estado
- 📢 **Módulo de Avisos** con reacciones sociales y filtrado por fecha
- 🔔 **Notificaciones en tiempo real** para actualizaciones de dashboards y tickets
- 📁 **Documentación de dashboards** — PDFs adjuntos y screenshots por dashboard
- 🛡️ **Panel de Administración** con auditoría de acciones, gestión de usuarios, áreas, categorías y catálogos

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core 8 — Razor Pages |
| Base de datos | SQL Server (Stored Procedures como patrón principal) |
| ORM | EF Core + ADO.NET |
| Frontend | Bootstrap 5 + Bootstrap Icons |
| Tipografía | Google Fonts: Playfair Display + DM Sans |
| Autenticación | Cookie-based — sesiones de 8 horas |

---

## Estructura del proyecto

```
DataXcaret/
├── Data/
│   └── CentralDashboardsContext.cs
├── Helpers/
├── Models/
│   └── Dtos/
├── Pages/
│   ├── Account/            # Login / Logout
│   ├── Admin/              # Panel administrativo
│   │   ├── Areas/
│   │   ├── Auditoria/
│   │   ├── Catalogos/
│   │   ├── Dashboards/
│   │   ├── Incidencias/
│   │   ├── Solicitudes/
│   │   └── Usuarios/
│   ├── Avisos/
│   ├── Dashboards/
│   ├── Incidencias/
│   ├── Notificaciones/
│   └── Solicitudes/
└── wwwroot/
    ├── css/
    ├── js/
    └── images/
```

---

## Base de datos

El acceso a datos se realiza principalmente mediante **Stored Procedures** ejecutados vía `FromSqlRaw` o `SqlCommand`. Los procedimientos siguen la convención de nombres:

```
sp_[Entidad]_[Acción]
```

Ejemplos: `sp_Dashboards_ObtenerTodos`, `sp_Incidencias_Crear`, `sp_Usuarios_ObtenerPorId`

---

## Despliegue

La aplicación corre en la red corporativa de Grupo Xcaret:

```

```

> ⚠️ Acceso exclusivo desde la red interna de Grupo Xcaret.

---

## Desarrollado por

**Carlos Martínez** — Practicante, Área de Innovación y Optimización  
Grupo Xcaret · Generación 2026-1
