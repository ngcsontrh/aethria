# Aethria Web

Frontend SPA for Aethria, built with React 19, TypeScript, Vite, Mantine, TanStack Router, TanStack Query, SignalR, and i18next.

## Project Structure

```text
aethria.web/
├── public/                  # Static public assets
├── src/
│   ├── assets/              # Bundled images and UI assets
│   ├── components/          # Reusable UI components
│   ├── i18n/                # i18next setup and locale files
│   ├── pages/               # Feature pages and page-scoped hooks
│   ├── services/            # API clients, auth helpers, and streaming clients
│   ├── index.css            # Global styles
│   ├── main.tsx             # React application bootstrap
│   └── router.tsx           # TanStack Router route tree
├── index.html
├── package.json
└── vite.config.ts
```

## Main Features

- Email and Google authentication.
- Resource, mentor, quiz, roadmap, API key, chat, and notification screens.
- Localized English and Vietnamese UI.
- React Query powered API state and SignalR streaming updates.
- Mantine component system with light/dark color scheme support.

## Scripts

```bash
npm run dev
npm run build
npm run lint
npm run preview
```

Use `npm run dev` for local development. Use `npm run build` before shipping frontend changes.
