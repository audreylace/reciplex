import { defineConfig } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    babel({
      presets: [reactCompilerPreset()],
    }),
  ],
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
