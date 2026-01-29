const CACHE_NAME = 'privatetube-cache-v1';
const ASSETS = [
    '/',
    '/css/site.css',
    '/js/site.js',
    '/manifest.json',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/jquery/dist/jquery.min.js'
];

self.addEventListener('install', (e) => {
    e.waitUntil(
        caches.open(CACHE_NAME).then((cache) => cache.addAll(ASSETS))
    );
});

self.addEventListener('fetch', (e) => {
    e.respondWith(
        caches.match(e.request).then((response) => response || fetch(e.request))
    );
});
