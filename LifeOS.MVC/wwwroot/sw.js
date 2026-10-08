const CACHE = 'lifeos-shell-v1';
const ASSETS = ['/offline.html', '/icons/icon-192.png', '/icons/icon-512.png'];
self.addEventListener('install', event => {
    event.waitUntil(caches.open(CACHE).then(cache => cache.addAll(ASSETS)));
    self.skipWaiting();
});
self.addEventListener('activate', event => {
    event.waitUntil(caches.keys().then(keys => Promise.all(keys.filter(key => key.startsWith('lifeos-shell-') && key !== CACHE).map(key => caches.delete(key)))).then(() => self.clients.claim()));
});
self.addEventListener('fetch', event => {
    const request = event.request;
    if (request.method !== 'GET' || new URL(request.url).origin !== self.location.origin) return;
    if (request.mode === 'navigate') {
        // Never store personal pages, financial records, or forms with anti-forgery tokens.
        event.respondWith(fetch(request).catch(() => caches.match('/offline.html')));
    } else if (ASSETS.includes(new URL(request.url).pathname)) {
        event.respondWith(caches.match(request).then(cached => cached || fetch(request)));
    }
});
