import { fileURLToPath, URL } from 'url';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';
import dotenv from 'dotenv';

// Učitavanje varijabli iz .env fajla
dotenv.config();

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "clickbar_porudzbine.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit' }).status) {
        throw new Error("Could not create certificate.");
    }
}

const target = process.env.VITE_BACKEND_URL || 'https://localhost:44323';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        VitePWA({
            registerType: 'autoUpdate',
            includeAssets: ['favicon.svg', 'favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
            manifest: {
                name: 'ClickBar Porudzbine',
                short_name: 'ClickBar',
                description: 'ClickBar Porudzbine PWA',
                theme_color: '#ffffff',
                icons: [
                    {
                        src: 'pwa-192x192.png',
                        sizes: '192x192',
                        type: 'image/png'
                    },
                    {
                        src: 'pwa-512x512.png',
                        sizes: '512x512',
                        type: 'image/png'
                    }
                ]
            }
        })
    ],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        host: '0.0.0.0', // Omogućava pristup sa svih IP adresa
        proxy: {
            '/api': {
                target: target,
                changeOrigin: true,
                secure: false,
                configure: (proxy, options) => {
                    proxy.on('error', (err, req, res) => {
                        console.error('Proxy error:', err);
                    });
                    proxy.on('proxyReq', (proxyReq, req, res) => {
                        console.log('Request:', req.method, req.url);
                    });
                }
            }
        },
        port: 3000,
        strictPort: true,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    },
    esbuild: {
        loader: 'jsx',
        include: [
            'src/**/*.js',
            'src/**/*.jsx'
        ],
        exclude: [
            'node_modules'
        ],
    },
    optimizeDeps: {
        include: ['events']
    },
    build: {
        rollupOptions: {
            external: ['@awesome-cordova-plugins/printer'],
        },
    },
});