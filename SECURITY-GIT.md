# Seguridad de Git para un repositorio publico

## Antes de preparar un commit

1. Revisa el estado:

   ```powershell
   git status --short
   git diff
   ```

2. Agrega archivos de forma selectiva:

   ```powershell
   git add ruta/al/archivo
   git add -p
   ```

3. Revisa exactamente lo que entrara al commit:

   ```powershell
   git diff --staged
   ```

4. Solo entonces crea el commit:

   ```powershell
   git commit -m "Describe un cambio concreto"
   ```

`git add .` no es inseguro por si mismo, pero agrega todo lo nuevo y modificado
que no este ignorado. Usalo solamente despues de revisar `git status` y confirma
el resultado con `git diff --staged`.

## Nunca versionar

- `.env`, claves API, tokens, contrasenas o secretos JWT.
- Certificados y claves privadas: `.pfx`, `.p12`, `.pem`, `.key`.
- Configuracion personal: `appsettings.Development.json`, `*.user`.
- Bases de datos locales, copias de seguridad y volcados con datos reales.
- `node_modules`, `bin`, `obj`, `dist`, logs y archivos del IDE.
- Datos personales o de clientes que no sean necesarios para ejecutar el proyecto.

## Configuracion local de .NET

El proyecto tiene un `UserSecretsId`. Guarda secretos de desarrollo con:

```powershell
dotnet user-secrets set "Jwt:Key" "valor-secreto" --project backend/GlobAccountAPI
dotnet user-secrets set "Smtp:Host" "smtp.example.com" --project backend/GlobAccountAPI
dotnet user-secrets set "Smtp:Username" "sender@example.com" --project backend/GlobAccountAPI
dotnet user-secrets set "Smtp:Password" "valor-secreto" --project backend/GlobAccountAPI
dotnet user-secrets set "Smtp:RecipientEmail" "recipient@example.com" --project backend/GlobAccountAPI
dotnet user-secrets set "Captcha:SecretKey" "valor-secreto-turnstile" --project backend/GlobAccountAPI
dotnet user-secrets set "Captcha:Enabled" "true" --project backend/GlobAccountAPI
```

No copies valores reales al archivo de ejemplo. En produccion usa los secretos
del proveedor de despliegue o variables de entorno. Para ASP.NET Core, las
variables anidadas se escriben con doble guion bajo, por ejemplo
`Captcha__SecretKey`, `Smtp__Username` y `Smtp__RecipientEmail`.

La clave JWT debe tener al menos 32 bytes. Cuando se rota, todos los tokens
emitidos con la clave anterior deben considerarse invalidados.

## Formulario de contacto

El formulario usa Cloudflare Turnstile cuando `Captcha:Enabled` esta activo en
backend y `VITE_TURNSTILE_SITE_KEY` esta definido en frontend. La site key puede
ser publica; la secret key nunca debe entrar al repositorio.

En el frontend, configura la site key fuera de Git, por ejemplo en `.env.local`
o en las variables del hosting:

```powershell
VITE_TURNSTILE_SITE_KEY=site-key-publica
```

## Usuarios y contrasenas

El backend espera hashes compatibles con `PasswordHasher<User>`. No almacenes
contrasenas en texto plano. Si existen usuarios legacy, migralos a hashes y deja
`Auth:AllowLegacyPlainTextPasswords` en `false` en produccion.

## Si un secreto llega a GitHub

1. Revocalo o rotalo inmediatamente.
2. Eliminalo del codigo actual.
3. Limpia el historial con `git filter-repo` si es necesario.
4. Coordina el `force push` con quienes hayan clonado el repositorio.

Eliminar el archivo en un commit nuevo no elimina el secreto de los commits
anteriores.
