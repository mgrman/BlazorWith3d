import { defineConfig } from 'vite';

export default defineConfig({
    build: {
    rollupOptions: {
        input: {
            app: "src/app.js",
        },
        output: {
            entryFileNames: `assets/[name].js`,
            chunkFileNames: `assets/[name].js`,
            assetFileNames: `assets/[name].[ext]`
        }
    }
}
})