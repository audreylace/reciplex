import { defineConfig } from "vite";
import preact from "@preact/preset-vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [preact()],
  build: {},
  server: {
    proxy: {
      // Proxy requests starting with '/api' to your backend server
      "/api": {
        target: "http://localhost:1993", // The address of your backend server
        changeOrigin: true, // Needed for virtual hosted sites
        secure: false,
      },
    },
  },
});
