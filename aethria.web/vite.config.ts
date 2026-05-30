import { defineConfig } from "vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), babel({ presets: [reactCompilerPreset()] })],
  server: {
    port: 53174,
    headers: {
      "Cross-Origin-Opener-Policy": "same-origin-allow-popups",
    },
  },
  build: {
    chunkSizeWarningLimit: 1500,
    rolldownOptions: {
      checks: {
        invalidAnnotation: false,
      },
      output: {
        codeSplitting: {
          groups: [
            {
              name: "react-vendor",
              test: /node_modules[\\/](react|react-dom)[\\/]/,
            },
            { name: "mantine", test: /node_modules[\\/]@mantine[\\/]/ },
            { name: "tanstack", test: /node_modules[\\/]@tanstack[\\/]/ },
            {
              name: "mermaid-d3",
              test: /node_modules[\\/](d3|d3-.+)[\\/]/,
            },
            {
              name: "mermaid-layout",
              test: /node_modules[\\/](cytoscape|cytoscape-.+|dagre-d3-es|elkjs)[\\/]/,
            },
            {
              name: "mermaid-render",
              test: /node_modules[\\/](@iconify|@upsetjs|dompurify|katex|roughjs)[\\/]/,
            },
            {
              name: "i18n",
              test: /node_modules[\\/](i18next|react-i18next|i18next-browser-languagedetector)[\\/]/,
            },
            { name: "signalr", test: /node_modules[\\/]@microsoft[\\/]/ },
            {
              name: "vendor",
              test: /node_modules[\\/](?!mermaid[\\/])/,
            },
          ],
        },
      },
    },
  },
});
