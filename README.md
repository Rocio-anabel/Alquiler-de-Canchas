# Alquiler de Canchas

Sistema web MVC (ASP.NET Core) para la gestión del alquiler de canchas deportivas: administra canchas, clientes, turnos (reservas) y pagos, con acceso diferenciado por rol para el personal del complejo.

## Problema que resuelve

Los complejos deportivos suelen llevar las reservas en cuadernos o chats, lo que genera turnos superpuestos, pagos sin registrar y poca visibilidad de qué se cobró. Este sistema centraliza:

- El catálogo de **canchas** (tipo, superficie, techada, precio por hora) y sus fotos.
- La agenda de **turnos** por cancha y cliente, con su estado (Confirmado, Finalizado, Cancelado, Ausente).
- El registro de **pagos** (seña, pago total, reembolso) asociados a cada turno.
- La gestión de **empleados** y permisos por rol.

## Tecnologías

- ASP.NET Core MVC (C#), .NET 9
- MySQL (XAMPP) con ADO.NET (repositorios) y Entity Framework Core + Pomelo
- Autenticación por cookies (sitio web) y JWT Bearer (Web API)
- JavaScript con `fetch`/AJAX y Select2 en los módulos dinámicos

## Modelo de datos

Diagrama Entidad-Relación: [`/docs/der.png`](docs/der.png)

Entidades: `Usuario`, `Cliente`, `TipoCancha`, `Cancha`, `Imagen`, `Turno`, `Pago`.

- 1:N: `TipoCancha → Cancha`, `Cancha → Imagen`, `Turno → Pago`.
- `Turno` depende de otras tres entidades: `Cancha`, `Cliente` y `Usuario`.

## Cómo ejecutarlo en local

1. Iniciar **MySQL** desde XAMPP.
2. Importar `AlquilerDeCancha.sql` en phpMyAdmin (pestaña SQL). Crea la base `alquiler_canchas`, las tablas y los datos de prueba.
3. Revisar en `appsettings.json` la cadena de conexión (`ConnectionStrings:DefaultConnection`) y la sección `Jwt` (`Key` de al menos 32 caracteres, `Issuer`, `Audience`).
4. Ejecutar:
   ```bash
   dotnet restore
   dotnet run
   ```
5. Abrir la URL que indica la consola (por ejemplo `https://localhost:xxxx`).

## Usuarios de prueba

| Rol | Email | Contraseña |
|---|---|---|
| Administrador | `admin@canchas.com` | `Admin123!` |
| Empleado | `empleado@canchas.com` | `Empleado123!` |

- **Empleado**: gestiona clientes, turnos y pagos.
- **Administrador**: todo lo del empleado, y además gestiona empleados y canchas, y puede eliminar pagos.

## Matriz de evidencias de requerimientos mínimos

| Requerimiento mínimo | Estado | Enlace al código / controladores | Evidencia (captura / GIF / video) |
|---|---|---|---|
| 1. 6+ tablas y relación 1:N (Turno depende de Cancha + Cliente + Usuario) | ✅ Cumplido | [DataContext.cs](Models/DataContext.cs), [AlquilerDeCancha.sql](AlquilerDeCancha.sql) | ![DER](docs/der.png) <br> 7 tablas. 1:N: TipoCancha→Cancha, Cancha→Imagen, Turno→Pago. Turno depende de Cancha + Cliente + Usuario |
<<<<<<< HEAD
| 2. Login, Roles y Avatar | ✅ Cumplido | [Login/Logout y avatar: UsuarioController.cs](Controllers/UsuarioController.cs), [Cookie y rutas: Program.cs](Program.cs), [Restricción por rol: TipoCanchaController.cs]https://github.com/Rocio-anabel/Alquiler-de-Canchas/blob/main/Controllers/TipoCanchaController.cs#L37-L62 | ![Perfil con avatar](docs/perfil-avatar.png) <br> ![Acceso restringido](docs/rol-restringido.png) <br> Login con cookies y hash de contraseña; roles Administrador/Empleado; el alta, edición y baja de tipos de cancha y la gestión de usuarios son exclusivas del Administrador; avatar guardado en `wwwroot/Uploads/Avatares` |
| 3. Manejo de archivos (fotos de canchas) | ✅ Cumplido | [Create con subida de fotos](https://github.com/Rocio-anabel/Alquiler-de-Canchas/blob/main/Controllers/CanchaController.cs#L77-L116), [Validación, guardado y borrado: helpers](https://github.com/Rocio-anabel/Alquiler-de-Canchas/blob/main/Controllers/CanchaController.cs#L227-L270), [Imagen.cs](Models/Imagen.cs) | ![Crear cancha con fotos](docs/crear-cancha.png) <br> ![Editar con fotos guardadas](docs/edit-cancha.png) <br> ![Validación de archivo](docs/error-formato.png) <br> Subida múltiple de imágenes (JPG/PNG/WEBP, máx. 5 MB) guardadas en `wwwroot/Uploads/Canchas` con nombre GUID y registradas en la tabla `imagen`; baja de imagen borra registro y archivo; solo Administrador |
| 4. ABM completo con Vue.js + AJAX (Pagos) | ✅ Cumplido | [Views/Pago/index.cshtml](Views/Pago/index.cshtml), [PagoController.cs](Controllers/PagoController.cs) | ![Listado de pagos](docs/pagos.png) <br> ![Alta con búsqueda de turno](docs/pagos-modal-turno.png) <br> Vue 3: listado, alta, edición y baja por AJAX/JSON sin recargar; la baja es solo del Administrador |
| 5. Paginado por servidor | ✅ Cumplido | [PagoController.cs → Listar](Controllers/PagoController.cs), [CanchaController.cs → Index](Controllers/CanchaController.cs), [UsuarioController.cs → Index](Controllers/UsuarioController.cs) | ![Petición con pagina=2](docs/pagos-network-pagina2.png) <br> El servidor devuelve solo la página pedida (`pagina`, `tamPagina`) con el total; tipos de cancha queda sin paginar por ser una tabla maestra de 4 registros |
| 6. Selección asíncrona (AJAX) | ✅ Cumplido | [PagoController.cs → BuscarTurnos](Controllers/PagoController.cs), [Views/Pago/index.cshtml](Views/Pago/index.cshtml) | ![Búsqueda de turnos en el desplegable](docs/turnos.png) <br> Select2 consulta `/Pago/BuscarTurnos?term=...&pagina=...`, con carga progresiva y sin traer todos los registros |
| 7. Web API REST + JWT | ✅ Cumplido | [AuthApiController.cs](Controllers/Api/AuthApiController.cs), [PagosApiController.cs](Controllers/Api/PagosApiController.cs), [Program.cs](Program.cs) | ![Login: devuelve el token](docs/api-login.png) <br> ![Sin token: 401](docs/api-401-sin-token.png) <br> ![Con Bearer Token: 200](docs/api-200-con-token.png) <br> `POST /api/auth/login` devuelve un JWT; `GET /api/pagos` (paginado) exige `Authorization: Bearer <token>`. Colección de Postman en `docs/postman/` |
=======
| 2. Login, roles y avatar | 🟡 En progreso (login y roles listos; avatar a confirmar) | [UsuarioController.cs](Controllers/UsuarioController.cs), [Program.cs](Program.cs) | ![Captura de Avatar](docs/evidencias/perfil-avatar.png) |
| 3. Manejo de archivos (fotos de canchas) | 🟡 En progreso | [CanchaController.cs](Controllers/CanchaController.cs), [Imagen.cs](Models/Imagen.cs) | ![Cargar imagen de una cancha](docs/evidencias/cargarimagencancha.gif) |
| 4. ABM completo con Vue.js/AJAX (Pagos) | 🟡 En progreso (endpoints JSON listos) | [PagoController.cs](Controllers/PagoController.cs) (`Listar`, `Crear`, `Editar`, `Eliminar`), `Views/Pago/Index.cshtml` | _Pendiente: GIF del ABM sin recarga_ |
| 5. Paginado por servidor | ✅ Cumplido (Pagos) | [PagoController.cs → Listar](Controllers/PagoController.cs) | _Pendiente: captura del inspector de red con `pagina=2`_ |
| 6. Selección asíncrona de entidades relacionadas | ✅ Cumplido (Pagos → Turno) | [PagoController.cs → BuscarTurnos](Controllers/PagoController.cs) | _Pendiente: GIF del desplegable buscando turnos_ |
| 7. Web API + JWT | 🟡 En progreso (login JWT listo) | [AuthApiController.cs](Controllers/Api/AuthApiController.cs), [Program.cs](Program.cs) | _Pendiente: captura de Postman con Bearer Token_ |
>>>>>>> origin/main


## Estructura del repositorio

```
Controllers/        Controladores MVC y Api/ (JWT)
Models/             Entidades, DataContext y repositorios
Views/              Vistas Razor
wwwroot/            Recursos estáticos y archivos subidos
docs/               DER y colección de Postman
AlquilerDeCancha.sql  Script de base de datos con datos de prueba
```

