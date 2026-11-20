/* ==========================================================================
   CampusCare core behaviour (frontend foundation)

   1. Toasts          CampusCare.toast({ message, type, duration })
   2. Busy forms      <form data-cc-busy> ignores repeat submits
   3. Relative times  <time datetime="..." data-cc-relative>
   4. Flash messages  <div data-cc-flash="success">Message</div>

   No dependencies. Works alongside Bootstrap's own JavaScript.
   Nothing here talks to a server: it only changes what the page shows.
   ========================================================================== */
(function () {
  'use strict';

  var doc = document;

  /* ---- Helpers --------------------------------------------------------- */

  // The sprite URL comes from <html data-cc-icons="...">, so the app still
  // works when it is hosted in a virtual directory.
  function iconUrl() {
    return doc.documentElement.getAttribute('data-cc-icons') || '/img/campuscare/icons.svg';
  }

  function iconMarkup(name) {
    return '<svg class="cc-icon" aria-hidden="true" focusable="false"><use href="' +
      iconUrl() + '#cc-' + name + '"></use></svg>';
  }

  /* ---- 1. Toasts (spec 11) ---------------------------------------------
     Bottom inline-end. role=status (role=alert for errors). Stays at least
     8 seconds, pauses on hover and focus, has a dismiss button. Errors never
     auto-dismiss. The message is set with textContent, so it is always safe
     to pass user-entered text. */

  var TOAST_MS = 8000;
  var TOAST_ICONS = {
    success: 'check-circle',
    info: 'info-circle',
    warning: 'exclamation-triangle',
    danger: 'exclamation-circle'
  };

  function toastRegion() {
    var region = doc.getElementById('cc-toast-region');
    if (!region) {
      region = doc.createElement('div');
      region.id = 'cc-toast-region';
      region.className = 'cc-toast-region';
      doc.body.appendChild(region);
    }
    return region;
  }

  function toast(options) {
    options = options || {};
    var type = TOAST_ICONS[options.type] ? options.type : 'info';

    var el = doc.createElement('div');
    el.className = 'cc-toast cc-toast--' + type;
    el.setAttribute('role', type === 'danger' ? 'alert' : 'status');

    var icon = doc.createElement('span');
    icon.className = 'cc-toast__icon';
    icon.innerHTML = iconMarkup(TOAST_ICONS[type]);

    var message = doc.createElement('p');
    message.className = 'cc-toast__message';
    message.textContent = options.message || '';

    var close = doc.createElement('button');
    close.type = 'button';
    close.className = 'cc-toast__close';
    close.setAttribute('aria-label', 'Dismiss notification');
    close.innerHTML = iconMarkup('x');

    el.appendChild(icon);
    el.appendChild(message);
    el.appendChild(close);
    toastRegion().appendChild(el);

    var persistent = type === 'danger' || options.persistent === true;
    var remaining = Math.max(options.duration || TOAST_MS, TOAST_MS);
    var startedAt = 0;
    var timer = null;

    function dismiss() {
      if (timer) { clearTimeout(timer); timer = null; }
      if (el.parentNode) { el.parentNode.removeChild(el); }
    }

    function start() {
      if (persistent || timer) { return; }
      startedAt = Date.now();
      timer = setTimeout(dismiss, remaining);
    }

    function pause() {
      if (!timer) { return; }
      clearTimeout(timer);
      timer = null;
      // After a pause, always leave a few seconds to finish reading.
      remaining = Math.max(remaining - (Date.now() - startedAt), 3000);
    }

    close.addEventListener('click', dismiss);
    el.addEventListener('mouseenter', pause);
    el.addEventListener('mouseleave', start);
    el.addEventListener('focusin', pause);
    el.addEventListener('focusout', function (e) {
      if (!el.contains(e.relatedTarget)) { start(); }
    });

    start();
    return { element: el, dismiss: dismiss };
  }

  /* Server-rendered messages (for example from TempData) become toasts:
       <div data-cc-flash="success">Ticket CC-0143 created.</div>
     Without JavaScript the text simply stays visible on the page. */
  function upgradeFlashMessages() {
    var nodes = doc.querySelectorAll('[data-cc-flash]');
    Array.prototype.forEach.call(nodes, function (node) {
      var text = (node.textContent || '').trim();
      if (text) {
        toast({ type: node.getAttribute('data-cc-flash'), message: text });
      }
      if (node.parentNode) { node.parentNode.removeChild(node); }
    });
  }

  /* Any element with data-cc-toast-message fires a toast on click. Used by
     the design-system preview and handy for prototypes while the backend is
     not connected. It does not save anything. */
  doc.addEventListener('click', function (e) {
    var trigger = e.target.closest ? e.target.closest('[data-cc-toast-message]') : null;
    if (!trigger) { return; }
    toast({
      type: trigger.getAttribute('data-cc-toast-type') || 'info',
      message: trigger.getAttribute('data-cc-toast-message')
    });
  });

  /* ---- 2. Busy forms (spec 11, 12) --------------------------------------
     After a valid submit the buttons show a busy state and repeat submits
     are ignored. The work happens in a timeout so the browser has already
     collected the form data (including which button was pressed). If the
     browser's or jQuery's validation blocks the submit, nothing changes. */

  function setBusy(form, submitter) {
    form.setAttribute('data-cc-submitting', 'true');
    var buttons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
    Array.prototype.forEach.call(buttons, function (button) {
      button.classList.add('is-busy');
      button.setAttribute('aria-disabled', 'true');
    });
    if (submitter && submitter.tagName === 'BUTTON') {
      var originalHtml = submitter.innerHTML;
      var text = submitter.getAttribute('data-cc-busy-label') || stripTags(originalHtml) || 'Saving';
      submitter.setAttribute('data-cc-original', originalHtml);
      submitter.innerHTML = '<span class="cc-spinner" aria-hidden="true"></span><span></span>';
      submitter.lastChild.textContent = text;
    }
  }

  function stripTags(html) {
    var holder = doc.createElement('div');
    holder.innerHTML = html;
    return (holder.textContent || '').trim();
  }

  function clearBusy(form) {
    form.removeAttribute('data-cc-submitting');
    var buttons = form.querySelectorAll('.is-busy');
    Array.prototype.forEach.call(buttons, function (button) {
      button.classList.remove('is-busy');
      button.removeAttribute('aria-disabled');
      var original = button.getAttribute('data-cc-original');
      if (original !== null) {
        button.innerHTML = original;
        button.removeAttribute('data-cc-original');
      }
    });
  }

  doc.addEventListener('submit', function (e) {
    var form = e.target;
    if (!form || !form.hasAttribute || !form.hasAttribute('data-cc-busy')) { return; }

    if (form.getAttribute('data-cc-submitting') === 'true') {
      e.preventDefault();
      return;
    }

    var submitter = e.submitter || null;
    setTimeout(function () {
      if (e.defaultPrevented) { return; }
      setBusy(form, submitter);
    }, 0);
  });

  // Returning with the browser's Back button restores the page from cache:
  // make sure no button is left stuck in the busy state.
  window.addEventListener('pageshow', function (e) {
    if (!e.persisted) { return; }
    var forms = doc.querySelectorAll('form[data-cc-busy]');
    Array.prototype.forEach.call(forms, clearBusy);
  });

  /* ---- 3. Relative times (spec 10.4, 12) ---------------------------------
     <time datetime="2026-11-16T08:30:00Z" data-cc-relative>16 Nov 2026, 10:30</time>
     The visible text becomes "2h ago". The original absolute text stays
     available to screen readers and in the title attribute. */

  function relativeText(date) {
    var seconds = Math.max(0, Math.round((Date.now() - date.getTime()) / 1000));
    if (seconds < 60) { return 'Just now'; }
    var minutes = Math.floor(seconds / 60);
    if (minutes < 60) { return minutes + 'm ago'; }
    var hours = Math.floor(minutes / 60);
    if (hours < 24) { return hours + 'h ago'; }
    var days = Math.floor(hours / 24);
    if (days < 30) { return days + 'd ago'; }
    return date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  function refreshTimes() {
    var nodes = doc.querySelectorAll('time[data-cc-relative]');
    Array.prototype.forEach.call(nodes, function (node) {
      var date = new Date(node.getAttribute('datetime'));
      if (isNaN(date.getTime())) { return; }

      var absolute = node.getAttribute('data-cc-absolute');
      if (absolute === null) {
        absolute = (node.textContent || '').trim();
        node.setAttribute('data-cc-absolute', absolute);
        node.setAttribute('title', absolute);
        node.innerHTML = '<span data-cc-rel aria-hidden="true"></span>' +
          '<span class="cc-visually-hidden" data-cc-abs></span>';
        node.querySelector('[data-cc-abs]').textContent = absolute;
      }
      node.querySelector('[data-cc-rel]').textContent = relativeText(date);
    });
  }

  /* ---- Start-up --------------------------------------------------------- */

  function init() {
    upgradeFlashMessages();
    refreshTimes();
    setInterval(refreshTimes, 60000);
  }

  if (doc.readyState === 'loading') {
    doc.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }

  window.CampusCare = window.CampusCare || {};
  window.CampusCare.toast = toast;
  window.CampusCare.refreshTimes = refreshTimes;
})();
