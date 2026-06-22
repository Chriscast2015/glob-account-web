# Global Account Services Frontend

Frontend en React + Vite para la landing page y el formulario de contacto de Global Account Services.

## Ubicacion del proyecto

La app real vive en:

```bash
frontend/login-app
```

El `package.json` de `frontend` funciona como wrapper, por lo que tambien puedes ejecutar los scripts desde `frontend`.

## Scripts

Desde `frontend`:

```bash
npm run dev
npm run build
npm run lint
npm run preview
```

Desde `frontend/login-app`:

```bash
npm run dev
npm run build
npm run lint
npm run preview
```

## Desarrollo local

El servidor de Vite usa el puerto `5173`.

```bash
npm run dev
```

La ruta `/api` se proxya hacia el backend configurado en `vite.config.js` para integraciones como el formulario de contacto:

```js
target: "http://localhost:5093"
```

## Rutas

- `/`: landing publica.

Las rutas antiguas o no disponibles redirigen a la landing.
