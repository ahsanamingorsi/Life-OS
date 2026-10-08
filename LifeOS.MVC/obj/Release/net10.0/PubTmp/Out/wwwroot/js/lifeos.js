/* ════════════════════════════════════════════════════════════════
   LIFE OS v2.0 — Main JS
   ════════════════════════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {
    initNavHighlight();
    initProgressBars();
    initAlertDismiss();
    initScoreRings();
    initMobileNav();
    initFormEnhancements();
    initConfirmActions();
    initClock();
    initKeyboardShortcuts();
    initTimer();
});

/* ── ACTIVE NAV ───────────────────────────────────────────────── */
function initNavHighlight() {
    const path = '/' + (window.location.pathname.split('/').filter(Boolean)[0] || '');
    document.querySelectorAll('.los-nav-link').forEach(link => {
        const href = (link.getAttribute('href') || '').toLowerCase();
        if (href === '/' && (path === '' || path === '/')) {
            link.classList.add('active');
        } else if (href !== '/' && path.toLowerCase().startsWith(href)) {
            link.classList.add('active');
        }
    });
}

/* ── PROGRESS BARS ────────────────────────────────────────────── */
function initProgressBars() {
    const io = new IntersectionObserver(entries => {
        entries.forEach(e => {
            if (!e.isIntersecting) return;
            const bar = e.target;
            const w = parseFloat(bar.dataset.width || 0);
            bar.style.width = '0%';
            requestAnimationFrame(() => {
                setTimeout(() => { bar.style.width = w + '%'; }, 80);
            });
            io.unobserve(bar);
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.progress-bar[data-width]').forEach(bar => {
        bar.style.width = '0%';
        io.observe(bar);
    });
}

/* ── ALERTS ───────────────────────────────────────────────────── */
function initAlertDismiss() {
    document.querySelectorAll('.alert').forEach(alert => {
        alert.style.cursor = 'pointer';
        alert.title = 'Click to dismiss';

        const hide = () => {
            alert.style.transition = 'opacity 0.3s ease, max-height 0.3s ease, margin 0.3s ease';
            alert.style.opacity = '0';
            alert.style.maxHeight = '0';
            alert.style.overflow = 'hidden';
            alert.style.marginBottom = '0';
            alert.style.padding = '0';
            setTimeout(() => alert.remove(), 350);
        };

        setTimeout(hide, 4500);
        alert.addEventListener('click', hide);
    });
}

/* ── SCORE RINGS ──────────────────────────────────────────────── */
function initScoreRings() {
    document.querySelectorAll('.score-ring-svg[data-score]').forEach(svg => {
        const circle = svg.querySelector('.ring-prog');
        if (!circle) return;
        const score = parseFloat(svg.dataset.score || 0);
        const r = parseFloat(circle.getAttribute('r'));
        const circ = 2 * Math.PI * r;
        circle.style.strokeDasharray = circ;
        circle.style.strokeDashoffset = circ;

        setTimeout(() => {
            const offset = circ - (score / 100) * circ;
            circle.style.strokeDashoffset = offset;

            // Color by score
            const color = score >= 75 ? 'var(--green)'
                : score >= 45 ? 'var(--amber)'
                    : 'var(--rose)';
            circle.style.stroke = color;
        }, 250);
    });
}

/* ── MOBILE NAV ───────────────────────────────────────────────── */
function initMobileNav() {
    const toggle = document.getElementById('navToggle');
    const nav = document.querySelector('.los-nav');
    const overlay = document.getElementById('navOverlay');

    const open = () => { nav?.classList.add('mobile-open'); overlay?.classList.add('active'); document.body.style.overflow = 'hidden'; };
    const close = () => { nav?.classList.remove('mobile-open'); overlay?.classList.remove('active'); document.body.style.overflow = ''; };

    toggle?.addEventListener('click', open);
    overlay?.addEventListener('click', close);

    // Close on nav link click (mobile)
    nav?.querySelectorAll('.los-nav-link').forEach(link => {
        link.addEventListener('click', () => {
            if (window.innerWidth <= 1024) close();
        });
    });
}

/* ── FORM ENHANCEMENTS ────────────────────────────────────────── */
function initFormEnhancements() {
    // Auto-resize textareas
    document.querySelectorAll('textarea.form-control').forEach(ta => {
        const resize = () => { ta.style.height = 'auto'; ta.style.height = Math.max(80, ta.scrollHeight) + 'px'; };
        ta.addEventListener('input', resize);
        resize();
    });
}

/* ── CONFIRM ACTIONS ──────────────────────────────────────────── */
function initConfirmActions() {
    document.querySelectorAll('[data-confirm]').forEach(el => {
        el.addEventListener('click', e => {
            if (!confirm(el.dataset.confirm || 'Are you sure?')) e.preventDefault();
        });
    });
}

/* ── LIVE CLOCK ───────────────────────────────────────────────── */
function initClock() {
    const el = document.getElementById('liveClock');
    if (!el) return;

    function tick() {
        const now = new Date();
        let h = now.getHours();
        const m = String(now.getMinutes()).padStart(2, '0');
        const ampm = h >= 12 ? 'PM' : 'AM';
        h = h % 12 || 12;
        el.textContent = `${h}:${m} ${ampm}`;
    }
    tick();
    setInterval(tick, 1000);
}

/* ── KEYBOARD SHORTCUTS ───────────────────────────────────────── */
function initKeyboardShortcuts() {
    document.addEventListener('keydown', e => {
        const tag = document.activeElement?.tagName;
        const typing = tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT';

        // Ctrl+K → search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            document.getElementById('globalSearch')?.focus();
        }

        // Ctrl+Space → timer
        if (e.ctrlKey && e.key === ' ' && !typing) {
            e.preventDefault();
            window.timerRunning ? window.tPause() : window.tStart();
            document.getElementById('timerWidget')?.classList.add('open');
        }

        // Escape → close timer / modal
        if (e.key === 'Escape') {
            document.getElementById('timerWidget')?.classList.remove('open');
        }
    });
}

/* ══════════════════════════════════════════════════════════════
   FOCUS TIMER
   ══════════════════════════════════════════════════════════════ */
function initTimer() {
    let totalSec = 25 * 60;
    let remaining = totalSec;
    let tickId = null;
    let doneOnce = false;
    window.timerRunning = false;

    const CIRC = 2 * Math.PI * 58;

    const $ = id => document.getElementById(id);

    function fmt(s) {
        return String(Math.floor(s / 60)).padStart(2, '0') + ':' + String(s % 60).padStart(2, '0');
    }

    function updateUI() {
        const disp = $('tDisplay');
        const status = $('tStatus');
        const ring = $('tRing');
        const btn = $('timerToggleBtn');
        const label = $('timerBtnLabel');

        if (!disp) return;

        disp.textContent = fmt(remaining);

        const pct = remaining / totalSec;
        const offset = CIRC * (1 - pct);
        if (ring) {
            ring.style.strokeDasharray = CIRC;
            ring.style.strokeDashoffset = offset;
            ring.style.stroke = remaining <= 60
                ? 'var(--rose)'
                : remaining <= 300
                    ? 'var(--amber)'
                    : 'var(--indigo)';
        }

        disp.className = 't-display' + (
            remaining === 0 ? ' finished' :
                window.timerRunning && remaining <= 60 ? ' warning' :
                    window.timerRunning ? ' running' :
                        remaining < totalSec ? ' paused' : ''
        );

        if (status) status.textContent =
            remaining === 0 ? '✓ Done!' :
                window.timerRunning ? 'Focusing…' :
                    remaining < totalSec ? 'Paused' : 'Ready';

        if (btn) {
            btn.className = 'los-timer-btn' + (window.timerRunning ? ' running' : remaining < totalSec && remaining > 0 ? ' active' : '');
        }
        if (label) {
            label.textContent = window.timerRunning || (remaining < totalSec && remaining > 0 && !window.timerRunning)
                ? fmt(remaining) : 'Focus';
        }
    }

    window.timerToggle = () => {
        const w = $('timerWidget');
        const b = $('timerBackdrop');
        w?.classList.toggle('open');
        b?.classList.toggle('active');
    };

    window.timerClose = () => {
        $('timerWidget')?.classList.remove('open');
        $('timerBackdrop')?.classList.remove('active');
    };

    window.tStart = () => {
        if (remaining === 0) window.tReset();
        window.timerRunning = true;
        doneOnce = false;
        const sb = $('tStartBtn');
        if (sb) sb.textContent = '▶ Running';
        clearInterval(tickId);
        tickId = setInterval(() => {
            if (remaining > 0) { remaining--; updateUI(); }
            else { clearInterval(tickId); window.timerRunning = false; if (!doneOnce) { doneOnce = true; onDone(); } updateUI(); }
        }, 1000);
        updateUI();
    };

    window.tPause = () => {
        clearInterval(tickId);
        window.timerRunning = false;
        const sb = $('tStartBtn');
        if (sb) sb.textContent = '▶ Resume';
        updateUI();
    };

    window.tReset = () => {
        clearInterval(tickId);
        window.timerRunning = false;
        remaining = totalSec;
        doneOnce = false;
        const sb = $('tStartBtn');
        if (sb) sb.textContent = '▶ Start';
        updateUI();
    };

    window.tSetPreset = (minutes, el) => {
        document.querySelectorAll('.t-preset').forEach(b => b.classList.remove('on'));
        el.classList.add('on');
        $('tCustomWrap').style.display = 'none';
        totalSec = minutes * 60;
        remaining = totalSec;
        clearInterval(tickId);
        window.timerRunning = false;
        const sb = $('tStartBtn');
        if (sb) sb.textContent = '▶ Start';
        updateUI();
    };

    window.tShowCustom = () => {
        document.querySelectorAll('.t-preset').forEach(b => b.classList.remove('on'));
        $('tCustomBtn').classList.add('on');
        const cw = $('tCustomWrap');
        cw.style.display = cw.style.display === 'none' ? 'block' : 'none';
    };

    window.tApplyCustom = () => {
        const val = parseInt($('tCustomMin')?.value) || 30;
        totalSec = Math.max(1, Math.min(300, val)) * 60;
        remaining = totalSec;
        $('tCustomWrap').style.display = 'none';
        clearInterval(tickId);
        window.timerRunning = false;
        const sb = $('tStartBtn');
        if (sb) sb.textContent = '▶ Start';
        updateUI();
    };

    function onDone() {
        const label = $('tLabel')?.value || 'Focus session';
        const mins = Math.round(totalSec / 60);

        // Notification
        if ('Notification' in window) {
            const send = () => new Notification('⏱ Session Complete!', {
                body: `${label} — ${mins}m done. Take a break!`
            });
            Notification.permission === 'granted' ? send()
                : Notification.permission !== 'denied' && Notification.requestPermission().then(p => p === 'granted' && send());
        }

        // Tab flash
        const orig = document.title;
        let flash = true;
        const fi = setInterval(() => { document.title = flash ? '⏱ Done! — ' + orig : orig; flash = !flash; }, 700);
        setTimeout(() => { clearInterval(fi); document.title = orig; }, 6000);

        // Glow
        const w = $('timerWidget');
        if (w) { w.style.boxShadow = '0 0 0 4px rgba(16,185,129,0.25), var(--shadow-lg)'; setTimeout(() => { w.style.boxShadow = 'var(--shadow-lg)'; }, 3000); }

        const sb = $('tStartBtn');
        if (sb) sb.textContent = '▶ Start Again';
    }

    updateUI();
}

/* ── VAULT HELPERS ────────────────────────────────────────────── */
window.revealPw = (id, btn) => {
    const el = document.getElementById('pw' + id);
    if (!el) return;
    const hidden = !el.classList.contains('revealed');
    if (hidden) {
        el.textContent = el.dataset.plain || '(empty)';
        el.classList.add('revealed');
        btn.textContent = '🙈';
        setTimeout(() => { el.textContent = '••••••••'; el.classList.remove('revealed'); btn.textContent = '👁'; }, 8000);
    } else {
        el.textContent = '••••••••';
        el.classList.remove('revealed');
        btn.textContent = '👁';
    }
};

window.copyText = (text, btn) => {
    if (!text) return;
    navigator.clipboard.writeText(text).then(() => {
        const orig = btn.textContent;
        btn.textContent = '✓';
        btn.style.color = 'var(--green)';
        setTimeout(() => { btn.textContent = orig; btn.style.color = ''; }, 1500);
    });
};