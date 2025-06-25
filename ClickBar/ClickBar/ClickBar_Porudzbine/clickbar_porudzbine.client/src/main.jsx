import React from 'react';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.jsx';
import { registerSW } from 'virtual:pwa-register';

registerSW();

let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
    const installBtn = document.createElement('button');
    installBtn.id = 'install-btn';
    installBtn.textContent = 'Install App';
    installBtn.style.position = 'fixed';
    installBtn.style.top = '10px';
    installBtn.style.right = '10px';
    installBtn.style.display = 'block';
    installBtn.onclick = async () => {
        deferredPrompt.prompt();
        const { outcome } = await deferredPrompt.userChoice;
        console.log(`User response to the install prompt: ${outcome}`);
        deferredPrompt = null;
        installBtn.style.display = 'none';
    };
    document.body.appendChild(installBtn);
}, { once: true });

window.addEventListener('appinstalled', () => {
    console.log('PWA installed');
    const installBtn = document.getElementById('install-btn');
    if (installBtn) {
        installBtn.remove();
    }
});

createRoot(document.getElementById('root')).render(
    <StrictMode>
        <BrowserRouter>
            <App />
        </BrowserRouter>
    </StrictMode>
);