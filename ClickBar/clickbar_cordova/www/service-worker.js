self.addEventListener('install', event => {
    console.log('Service Worker installing.');
    event.waitUntil(
        caches.open('static-cache-v1').then(cache => {
            return cache.addAll([
                '/',
                '/index.html',
                '/manifest.json',
                '/icon.png'
            ]);
        })
    );
});

self.addEventListener('activate', event => {
    console.log('Service Worker activating.');
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});