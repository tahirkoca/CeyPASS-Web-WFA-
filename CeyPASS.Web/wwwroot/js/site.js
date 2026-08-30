(function (window, document, $) {
    'use strict';

    var CeyPASS = window.CeyPASS || {};

    /* ---- Filter persistence ---- */
    CeyPASS.filters = {
        load: function (key) {
            try {
                return JSON.parse(localStorage.getItem('ceypass.filters.' + key) || 'null');
            } catch (e) {
                return null;
            }
        },
        save: function (key, obj) {
            localStorage.setItem('ceypass.filters.' + key, JSON.stringify(obj));
        },
        /**
         * Persist named form fields and optionally restore on first visit (empty query).
         * @param {string} key storage key
         * @param {HTMLFormElement|string} formEl form or selector
         * @param {string[]} fields field names to persist
         * @param {{ autoSubmit?: boolean }} [opts]
         */
        bindForm: function (key, formEl, fields, opts) {
            opts = opts || {};
            var form = typeof formEl === 'string' ? document.querySelector(formEl) : formEl;
            if (!form || !fields || !fields.length) return;

            var appliedFlag = 'ceypass.filters.applied.' + key;

            function fieldElements(name) {
                return form.querySelectorAll('[name="' + name + '"]');
            }

            function collect() {
                var obj = {};
                fields.forEach(function (name) {
                    var els = fieldElements(name);
                    if (!els.length) return;
                    var first = els[0];
                    if (first.type === 'checkbox') {
                        if (els.length === 1) {
                            obj[name] = first.checked ? (first.value || 'true') : '';
                        } else {
                            obj[name] = Array.prototype.filter.call(els, function (el) { return el.checked; })
                                .map(function (el) { return el.value; });
                        }
                    } else if (first.type === 'radio') {
                        var checked = Array.prototype.find.call(els, function (el) { return el.checked; });
                        obj[name] = checked ? checked.value : '';
                    } else {
                        obj[name] = first.value;
                    }
                });
                return obj;
            }

            function apply(obj) {
                if (!obj) return false;
                var changed = false;
                fields.forEach(function (name) {
                    if (!Object.prototype.hasOwnProperty.call(obj, name)) return;
                    var val = obj[name];
                    var els = fieldElements(name);
                    if (!els.length) return;
                    var first = els[0];
                    if (first.type === 'checkbox') {
                        if (els.length === 1) {
                            var should = !!val && val !== 'false' && val !== '0';
                            if (first.checked !== should) {
                                first.checked = should;
                                changed = true;
                            }
                        } else {
                            var set = Array.isArray(val) ? val.map(String) : String(val || '').split(',').filter(Boolean);
                            Array.prototype.forEach.call(els, function (el) {
                                var next = set.indexOf(String(el.value)) >= 0;
                                if (el.checked !== next) {
                                    el.checked = next;
                                    changed = true;
                                }
                            });
                        }
                    } else if (first.type === 'radio') {
                        Array.prototype.forEach.call(els, function (el) {
                            var next = String(el.value) === String(val);
                            if (el.checked !== next) {
                                el.checked = next;
                                changed = true;
                            }
                        });
                    } else {
                        var nextVal = val == null ? '' : String(val);
                        if (first.value !== nextVal) {
                            first.value = nextVal;
                            changed = true;
                        }
                    }
                });
                return changed;
            }

            function hasMeaningfulQuery() {
                var params = new URLSearchParams(window.location.search);
                return fields.some(function (name) {
                    var v = params.get(name);
                    return v != null && String(v).trim() !== '';
                });
            }

            function persist() {
                CeyPASS.filters.save(key, collect());
            }

            form.addEventListener('submit', persist);

            if (hasMeaningfulQuery()) {
                persist();
                sessionStorage.setItem(appliedFlag, '1');
                return;
            }

            if (sessionStorage.getItem(appliedFlag)) return;

            var saved = CeyPASS.filters.load(key);
            if (!saved) {
                sessionStorage.setItem(appliedFlag, '1');
                return;
            }

            var changed = apply(saved);
            sessionStorage.setItem(appliedFlag, '1');
            if (changed && opts.autoSubmit !== false) {
                if (typeof CeyPASS.busy !== 'undefined') {
                    CeyPASS.busy.show('Yükleniyor...');
                }
                // Defer so other DOM-ready handlers can finish wiring dependent dropdowns.
                setTimeout(function () { form.submit(); }, 0);
            }
        }
    };

    /* ---- Busy overlay ---- */
    function ensureBusyOverlay() {
        var el = document.getElementById('ceypassBusyOverlay');
        if (el) return el;
        el = document.createElement('div');
        el.id = 'ceypassBusyOverlay';
        el.className = 'ceypass-busy-overlay';
        el.innerHTML =
            '<div class="ceypass-busy-panel">' +
            '<div class="ceypass-busy-spinner"></div>' +
            '<div class="ceypass-busy-title" id="ceypassBusyTitle">Yükleniyor...</div>' +
            '</div>';
        document.body.appendChild(el);
        return el;
    }

    var busyDepth = 0;
    CeyPASS.busy = {
        show: function (title) {
            var el = ensureBusyOverlay();
            var titleEl = document.getElementById('ceypassBusyTitle');
            if (titleEl) titleEl.textContent = title || 'Yükleniyor...';
            busyDepth++;
            el.classList.add('is-visible');
        },
        hide: function () {
            busyDepth = Math.max(0, busyDepth - 1);
            if (busyDepth > 0) return;
            var el = document.getElementById('ceypassBusyOverlay');
            if (!el) return;
            el.classList.remove('is-visible');
        },
        reset: function () {
            busyDepth = 0;
            var el = document.getElementById('ceypassBusyOverlay');
            if (!el) return;
            el.classList.remove('is-visible');
        }
    };

    /* ---- Shared .btn-sil-onay confirm ---- */
    CeyPASS.confirm = {
        init: function () {
            var silOnayModalEl = document.getElementById('silOnayModal');
            var silOnayModalBody = document.getElementById('silOnayModalBody');
            var silOnayModalSilBtn = document.getElementById('silOnayModalSilBtn');
            if (!silOnayModalEl || !silOnayModalBody || !silOnayModalSilBtn) return;
            if (silOnayModalEl.getAttribute('data-ceypass-confirm-bound') === '1') return;
            silOnayModalEl.setAttribute('data-ceypass-confirm-bound', '1');

            document.addEventListener('click', function (e) {
                var btn = e.target.closest('.btn-sil-onay');
                if (!btn) return;
                e.preventDefault();
                var form = btn.closest('form');
                var msg = btn.getAttribute('data-msg') || 'Emin misiniz?';
                var btnText = btn.getAttribute('data-btn-text') || btn.getAttribute('title') || 'Onayla';
                var iconClass = btn.classList.contains('btn-success') ? 'bi-check-lg' : 'bi-trash';
                silOnayModalBody.textContent = msg;
                silOnayModalSilBtn.className = 'btn ' + (btn.classList.contains('btn-success') ? 'btn-success' : 'btn-danger');
                silOnayModalSilBtn.innerHTML = '<i class="bi ' + iconClass + ' me-1"></i>' + btnText;
                var modal = bootstrap.Modal.getOrCreateInstance(silOnayModalEl);
                silOnayModalSilBtn.onclick = function () {
                    modal.hide();
                    var undoUrl = btn.getAttribute('data-undo-url');
                    if (undoUrl && window.CeyPASS && CeyPASS.undo) {
                        CeyPASS.undo.queue({ url: undoUrl, fields: {} });
                    }
                    if (form) form.submit();
                };
                modal.show();
            });
        }
    };

    /* ---- Undo (Geri al, ~7 sn, tek pending) ---- */
    var UNDO_KEY = 'ceypass.pendingUndo';

    CeyPASS.undo = {
        queue: function (payload) {
            try {
                sessionStorage.setItem(UNDO_KEY, JSON.stringify(payload || {}));
            } catch (e) { /* ignore */ }
        },
        clear: function () {
            try { sessionStorage.removeItem(UNDO_KEY); } catch (e) { /* ignore */ }
        },
        getPending: function () {
            try {
                var raw = sessionStorage.getItem(UNDO_KEY);
                return raw ? JSON.parse(raw) : null;
            } catch (e) {
                return null;
            }
        },
        post: function (payload, done) {
            if (!payload || !payload.url) {
                if (done) done(false);
                return;
            }
            var token = document.querySelector('input[name="__RequestVerificationToken"]');
            var fd = new FormData();
            if (token && token.value) fd.append('__RequestVerificationToken', token.value);
            var fields = payload.fields || {};
            Object.keys(fields).forEach(function (k) {
                if (fields[k] != null) fd.append(k, fields[k]);
            });
            CeyPASS.busy.show('Geri alınıyor...');
            fetch(payload.url, { method: 'POST', body: fd, credentials: 'same-origin' })
                .then(function (r) {
                    CeyPASS.busy.hide();
                    CeyPASS.undo.clear();
                    if (r.redirected) {
                        window.location.href = r.url;
                    } else {
                        window.location.reload();
                    }
                    if (done) done(true);
                })
                .catch(function () {
                    CeyPASS.busy.hide();
                    if (window.toastr) toastr.error('Geri alma başarısız.');
                    if (done) done(false);
                });
        },
        offerToast: function (message) {
            if (!window.toastr) return;
            var pending = CeyPASS.undo.getPending();
            if (!pending || !pending.url) return;
            var payload = pending;
            CeyPASS.undo.clear();
            var undoId = 'ceypass-undo-' + Date.now();
            var html = (message || 'Tamamlandı') +
                ' <button type="button" class="btn btn-sm btn-light ms-2" id="' + undoId + '">Geri al</button>';
            toastr.success(html, '', {
                timeOut: 7000,
                extendedTimeOut: 2000,
                escapeHtml: false,
                closeButton: true,
                onclick: null
            });
            setTimeout(function () {
                var el = document.getElementById(undoId);
                if (!el) return;
                el.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    CeyPASS.undo.post(payload);
                });
            }, 0);
        },
        initRedirect: function () {
            var alert = document.querySelector('.alert.alert-success');
            if (!alert) return;
            var pending = CeyPASS.undo.getPending();
            if (!pending || !pending.url) return;
            var msg = (alert.textContent || '').trim().replace(/\s+/g, ' ');
            alert.classList.add('d-none');
            CeyPASS.undo.offerToast(msg);
        },
        offer: function (message, payload) {
            CeyPASS.undo.queue(payload);
            CeyPASS.undo.offerToast(message);
        }
    };

    /* ---- Status bar ---- */
    CeyPASS.status = {
        set: function (msg, count) {
            var msgEl = document.getElementById('ceypassStatusMsg');
            var countEl = document.getElementById('ceypassStatusCount');
            var text = (msg == null || String(msg).trim() === '') ? 'Hazır' : String(msg).trim();
            if (msgEl) msgEl.textContent = text;
            if (countEl) {
                if (count == null || count === '') {
                    countEl.textContent = '';
                    countEl.hidden = true;
                } else {
                    var n = Number(count);
                    countEl.textContent = isNaN(n)
                        ? String(count)
                        : (n === 1 ? '1 kayıt' : n + ' kayıt');
                    countEl.hidden = false;
                }
            }
        }
    };

    /* ---- Keyboard shortcuts modal ---- */
    var DEFAULT_SHORTCUTS = [
        { keys: 'Ctrl+/ veya F1', description: 'Bu kısayol listesini aç' },
        { keys: 'Esc', description: 'Diyalog / paneli kapat' },
        { keys: 'Ctrl+F', description: 'Tabloda ara (varsa)' },
        { keys: 'Ctrl+S', description: 'Kaydet (form ekranlarında)' }
    ];

    function parsePageShortcuts() {
        var raw = document.body.getAttribute('data-page-shortcuts');
        if (!raw || !String(raw).trim()) return [];
        try {
            var parsed = JSON.parse(raw);
            if (!Array.isArray(parsed)) return [];
            return parsed.filter(function (item) {
                return item && (item.keys || item.Keys) && (item.description || item.Description);
            }).map(function (item) {
                return {
                    keys: item.keys || item.Keys,
                    description: item.description || item.Description
                };
            });
        } catch (e) {
            return [];
        }
    }

    function renderShortcutList(items) {
        var list = document.getElementById('shortcutsModalList');
        if (!list) return;
        list.innerHTML = '';
        items.forEach(function (item) {
            var row = document.createElement('div');
            row.className = 'd-flex align-items-start gap-3 mb-2';
            row.innerHTML =
                '<kbd class="ceypass-shortcut-kbd">' + escapeHtml(item.keys) + '</kbd>' +
                '<span class="small">' + escapeHtml(item.description) + '</span>';
            list.appendChild(row);
        });
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    CeyPASS.shortcuts = {
        show: function () {
            var modalEl = document.getElementById('shortcutsModal');
            if (!modalEl || typeof bootstrap === 'undefined') return;
            var items = DEFAULT_SHORTCUTS.concat(parsePageShortcuts());
            renderShortcutList(items);
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }
    };

    function isTypingTarget(el) {
        if (!el) return false;
        var tag = (el.tagName || '').toLowerCase();
        if (tag === 'input' || tag === 'textarea' || tag === 'select') return true;
        if (el.isContentEditable) return true;
        return false;
    }

    function initP2Keyboard() {
        document.addEventListener('keydown', function (e) {
            if (e.ctrlKey && (e.key === '/' || e.key === '?' || e.code === 'Slash' || e.code === 'NumpadDivide')) {
                e.preventDefault();
                CeyPASS.shortcuts.show();
                return;
            }
            if (!e.ctrlKey && !e.metaKey && !e.altKey && e.key === '?' && !isTypingTarget(e.target)) {
                e.preventDefault();
                CeyPASS.shortcuts.show();
            }
        });
    }

    function initShortcutsToggle() {
        var shortcutsBtn = document.getElementById('shortcutsToggleBtn');
        if (shortcutsBtn && shortcutsBtn.getAttribute('data-ceypass-sc-bound') !== '1') {
            shortcutsBtn.setAttribute('data-ceypass-sc-bound', '1');
            shortcutsBtn.addEventListener('click', function () { CeyPASS.shortcuts.show(); });
        }
    }

    function hookToastrStatus() {
        if (!window.toastr || typeof toastr.success !== 'function') return;
        if (toastr.__ceypassStatusHooked) return;
        toastr.__ceypassStatusHooked = true;
        var orig = toastr.success.bind(toastr);
        toastr.success = function (message, title, optionsOverride) {
            var statusMsg = typeof message === 'string' && message
                ? message
                : (typeof title === 'string' && title ? title : 'Tamamlandı');
            try { CeyPASS.status.set(statusMsg); } catch (e) { /* ignore */ }
            return orig(message, title, optionsOverride);
        };
    }

    /* ---- Bootstrap needs-validation ---- */
    function initNeedsValidation() {
        document.querySelectorAll('form.needs-validation').forEach(function (form) {
            if (form.getAttribute('data-ceypass-validation-bound') === '1') return;
            form.setAttribute('data-ceypass-validation-bound', '1');
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }

    function initBusyHooks() {
        ensureBusyOverlay();

        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (!form || !form.matches) return;
            if (form.matches('#filterForm, #raporForm')) {
                CeyPASS.busy.show('Yükleniyor...');
            }
        }, true);
        // Note: do not hook ajaxStart/Stop globally — layout notification polling would flash the overlay.
    }

    function onReady(fn) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', fn);
        } else {
            fn();
        }
    }

    onReady(function () {
        initBusyHooks();
        CeyPASS.confirm.init();
        initNeedsValidation();
        initShortcutsToggle();
        initP2Keyboard();
        hookToastrStatus();
        CeyPASS.undo.initRedirect();
        CeyPASS.status.set('Hazır');
    });

    window.CeyPASS = CeyPASS;
})(window, document, window.jQuery);
