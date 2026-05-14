(function () {
  const STORAGE_KEY = 'heritage-theme';

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);

    document.querySelectorAll('[data-theme-toggle]').forEach(function (btn) {
      var isDark = theme === 'dark';

      // Metin güncelle
      btn.textContent = '';

      // SVG ikon — Güneş (açık mod) veya Ay (koyu mod)
      var icon = document.createElement('span');
      icon.setAttribute('aria-hidden', 'true');
      icon.innerHTML = isDark
        ? '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align:-2px;margin-right:6px"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>'
        : '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align:-2px;margin-right:6px"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>';

      var label = document.createTextNode(isDark ? 'Aydınlık' : 'Karanlık');
      btn.appendChild(icon);
      btn.appendChild(label);
      btn.setAttribute('aria-label', isDark ? 'Açık temaya geç' : 'Koyu temaya geç');
      btn.setAttribute('title', isDark ? 'Açık tema' : 'Koyu tema');
    });
  }

  // Sistem tercihini al
  function getSystemTheme() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    return 'light';
  }

  // Kayıtlı temayı veya sistem tercihini uygula
  var savedTheme = localStorage.getItem(STORAGE_KEY);
  applyTheme(savedTheme || getSystemTheme());

  // Tıklama ile değiştir
  document.addEventListener('click', function (e) {
    var btn = e.target.closest('[data-theme-toggle]');
    if (!btn) return;

    var current = document.documentElement.getAttribute('data-theme') || 'light';
    var next = current === 'dark' ? 'light' : 'dark';
    localStorage.setItem(STORAGE_KEY, next);
    applyTheme(next);
  });

  // Sistem tercihi değişirse (kayıtlı tema yoksa) otomatik güncelle
  if (window.matchMedia) {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
      if (!localStorage.getItem(STORAGE_KEY)) {
        applyTheme(e.matches ? 'dark' : 'light');
      }
    });
  }
})();
