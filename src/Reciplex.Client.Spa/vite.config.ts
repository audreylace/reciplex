import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import basicSsl from "@vitejs/plugin-basic-ssl";

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
