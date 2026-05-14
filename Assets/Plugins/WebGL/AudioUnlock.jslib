mergeInto(LibraryManager.library, {
  ResumeUnityAudioContext: function () {
    try {
      if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext) {
        if (WEBAudio.audioContext.state === 'suspended') {
          WEBAudio.audioContext.resume();
        }
      }
    } catch (e) {
      console.warn('ResumeUnityAudioContext failed:', e);
    }
  },
  SyncPersistentDataPathToIndexedDB: function () {
    try {
      if (typeof FS === 'undefined' || typeof FS.syncfs !== 'function') return;
      if (!FS.filesystems || !FS.filesystems.IDBFS) return;
      try {
        FS.syncfs(false, function (err) {
          if (err) {
            try { console.warn('FS.syncfs callback err:', err && err.message ? err.message : err); } catch (_) {}
          }
        });
      } catch (innerErr) {
        try { console.warn('FS.syncfs threw:', innerErr && innerErr.message ? innerErr.message : innerErr); } catch (_) {}
      }
    } catch (e) {
      try { console.warn('SyncPersistentDataPathToIndexedDB outer error'); } catch (_) {}
    }
  }
});
