// ZozoBlast service worker — caches the WebGL build for offline play.
// CACHE_VERSION is auto-rewritten to the git short SHA by scripts/deploy.sh.
const CACHE_VERSION = 'zozoblast-300d6d2';
// Unity WebGL build files are matched by prefix at fetch time (see below),
// not pre-listed here, so the cache keeps working if the Unity output name
// or sub-folder ever changes.
const PRECACHE_PATHS = [
  './',
  './index.html',
  './manifest.webmanifest',
  './favicon.png',
  './apple-touch-icon.png',
  './icon-192.png',
  './icon-512.png',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_VERSION).then((cache) => Promise.all(
      // Cache shell files individually so one missing icon doesn't abort the
      // whole install (which would silently leave the PWA without offline support).
      PRECACHE_PATHS.map((path) => cache.add(path).catch((err) => {
        console.warn('[sw] precache skip', path, err && err.message);
      }))
    )).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => Promise.all(
      keys.filter((key) => key !== CACHE_VERSION).map((key) => caches.delete(key))
    )).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  if (event.request.method !== 'GET') return;
  event.respondWith(
    caches.match(event.request).then((cached) => {
      if (cached) return cached;
      return fetch(event.request).then((response) => {
        if (!response || response.status !== 200 || response.type === 'opaque') {
          return response;
        }
        const cloned = response.clone();
        caches.open(CACHE_VERSION).then((cache) => cache.put(event.request, cloned));
        return response;
      }).catch(() => cached);
    })
  );
});
