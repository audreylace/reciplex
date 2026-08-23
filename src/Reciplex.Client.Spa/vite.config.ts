import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import basicSsl from "@vitejs/plugin-basic-ssl";
import { VitePWA } from "vite-plugin-pwa";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    basicSsl({
      /** name of certification */
      name: "test",
      /** custom trust domains */
      domains: ["localhost"],
      /** optional, days before certificate expires */
      ttlDays: 30,
    }),
    VitePWA({
      registerType: "autoUpdate",
      workbox: {
        // Precache build artifacts only
        globPatterns: ["**/*.{js,css,html,svg,woff2}"],

        // Exclude /api/ routes from being intercepted by the SPA fallback index.html
        navigateFallbackDenylist: [/^\/api/],

        runtimeCaching: [
          {
            // Explicitly force network-only for API requests (never check or save to cache)
            urlPattern: /^\/api\/.*$/i,
            handler: "NetworkOnly",
          },
        ],
      },
    }),
  ],
  build: {},
  server: {
    headers: {
      "x-content-type-options": "nosniff",
      "x-frame-options": "DENY",
    },
    proxy: {
      // Proxy requests starting with '/api' to your backend server
      "/api": {
        target: "http://localhost:1993", // The address of your backend server
        changeOrigin: false,
        secure: false,
        headers: {
          "X-Forwarded-Proto": "https",
          "X-Forwarded-Host": "localhost:5173",
        },
      },
    },
  },
});
