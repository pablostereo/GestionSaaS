# GestionSaaS – Stock, Ventas y Compras

| Proyecto        | Qué contiene                                                                 |
|-----------------|------------------------------------------------------------------------------|
| `GestionSaaS.Entidades` | Entidades, enums, constantes globales (`Comun/Constantes.cs`), `AppDbContext`, migraciones y datos demo (`Datos/`) |
| `GestionSaaS.Negocio`   | Servicios con la lógica (`Servicios/`), DTOs (`Dtos/`) y excepciones de negocio |
| `GestionSaaS.Api`       | Controllers REST. Solo llama a Negocio. Identifica al cliente por `X-Cliente-Id` |
| `GestionSaaS.View`      | React + Vite, pantalla básica para probar el sistema                          |

Dependencias: `GestionSaaS.Api → GestionSaaS.Negocio → GestionSaaS.Entidades`. La vista solo habla con la Api por HTTP.

## Cómo levantarlo

```bash
# 1) Api  (http://localhost:5024/swagger)
cd GestionSaaS.Api
dotnet run --launch-profile http

# 2) Vista (http://localhost:5173)
cd GestionSaaS.View
npm install      # solo la primera vez
npm run dev
```

En Visual Studio: elegir el perfil de inicio **Api + View** (combo junto al botón ▶) y F5 levanta las dos.
`GestionSaaS.View.esproj` es el proyecto de JavaScript que hace aparecer la vista en la solución.

## Base de datos

Se elige en `GestionSaaS.Api/appsettings.json` → `"BaseDatos": { "Proveedor": ... }`:

| Proveedor   | Dónde quedan los datos                                        |
|-------------|---------------------------------------------------------------|
| `Sqlite`    | **(por defecto)** archivo `GestionSaaS.Api/Datos/gestionsaas.db`. Se mantiene entre ejecuciones. |
| `Memoria`   | Solo RAM. Se borra al cerrar la Api.                          |
| `SqlServer` | Servidor real (`ConnectionStrings:SqlServer`).                |

Al arrancar con una base vacía se crean estos usuarios:

| Rol            | Email                     | Contraseña  | Qué puede hacer                              |
|----------------|---------------------------|-------------|----------------------------------------------|
| Administrador  | `admin@gestionsaas.local` | `Admin123!` | ABM de clientes (alta, edición, baja, contraseñas) |
| Cliente demo   | `demo@gestionsaas.local`  | `Demo123!`  | Productos, ventas y compras de su comercio (4 productos de ejemplo) |

La pantalla de login tiene botones para completar estos usuarios.

## Circuitos del cliente

| Pantalla        | Para qué                                                   | Toca stock |
|-----------------|------------------------------------------------------------|------------|
| **Compras**     | Reposición de mercadería: proveedor + productos (cantidad y costo). Suma stock y actualiza el costo de cada producto. | Sí |
| **Gastos**      | Egresos operativos (luz, alquiler, impuestos…): descripción, monto, fecha, categoría y medio de pago. Solo registra la salida de dinero. | No |
| **Proveedores** | A quién se le compra. No se borran: se desactivan (un inactivo no aparece al cargar compras). | No |

El SKU de los productos lo asigna el sistema: es correlativo por cliente (`000001`, `000002`…) y no se edita.
Dos clientes distintos pueden tener el mismo número, pero dentro de un cliente nunca se repite.

## Seguridad

- Login en `POST /api/auth/login` → devuelve un token JWT (8 horas).
- Todos los endpoints piden token. En Swagger: hacer login, copiar el `token` y pegarlo en **Authorize**.
- El cliente sale del token: un usuario nunca puede ver datos de otro cliente.
- Las contraseñas se guardan con hash (PBKDF2), nunca en texto plano.
- La clave de firma del token está en `appsettings.Development.json` (solo para desarrollo local).

- **Empezar de cero:** cerrar la Api y borrar la carpeta `GestionSaaS.Api/Datos`.
- **Ver el contenido:** abrir el `.db` con *DB Browser for SQLite* (gratis) o la extensión *SQLite Viewer* de VS Code.
- **Si se cambian las entidades:** con SQLite las tablas se crean desde el modelo (sin migraciones).
  La Api lo detecta al arrancar, mueve el `.db` viejo a `Datos/respaldo/` y crea uno nuevo.

## Pasar a SQL Server

1. Poner `"Proveedor": "SqlServer"` y completar `ConnectionStrings:SqlServer`.
2. Al arrancar, la Api aplica las migraciones sola. También se puede correr a mano:

```bash
cd GestionSaaS.Entidades
dotnet ef database update
```

## Nueva migración (después de cambiar entidades)

```bash
cd GestionSaaS.Entidades
dotnet ef migrations add NombreDelCambio -o Datos/Migraciones
```
