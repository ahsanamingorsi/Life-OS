/* ═══════════════════════════════════════════════
   LIFE OS — Main JS
   ═══════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {
    initNavHighlight();
    initProgressBars();
    initToastDismiss();
    initScoreRings();
    initMobileNav();
    initFormEnhancements();
    initConfirmForms();
});

/* ── ACTIVE NAV ─────────────────────────────────────────────────── */
function initNavHighlight() {
    const path = '/' + window.location.pathname.split('/').filter(Boolean)[0] ?? '';
    document.querySelectorAll('.los-nav-link').forEach(link => {
        const href = link.getAttribute('href') || '';
        if (href === '/' && (path === '' || path === '/')) {
            link.classList.add('active');
        } else if (href !== '/' && path.toLowerCase().startsWith(href.toLowerCase())) {
            link.classList.add('active');
        }
    });
}

/* ── PROGRESS BARS ──────────────────────────────────────────────── */
function initProgressBars() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const bar = entry.target;
                const target = parseFloat(bar.dataset.width || 0);
                bar.style.width = '0%';
                requestAnimationFrame(() => {
                    setTimeout(() => { bar.style.width = target + '%'; }, 60);
                });
                observer.unobserve(bar);
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.los-progress-bar[data-width]').forEach(bar => {
        bar.style.width = '0%';
        observer.observe(bar);
    });
}

/* ── TOAST / ALERT DISMISS ──────────────────────────────────────── */
function initToastDismiss() {
    document.querySelectorAll('.los-alert').forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.4s ease, margin 0.4s ease, max-height 0.4s ease';
            alert.style.opacity = '0';
            alert.style.maxHeight = '0';
            alert.style.overflow = 'hidden';
            alert.style.marginBottom = '0';
            setTimeout(() => alert.remove(), 450);
        }, 4000);

        // Click to dismiss
        alert.addEventListener('click', () => {
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        });
    });
}

/* ── SVG SCORE RINGS ─────────────────────────────────────────────── */
function initScoreRings() {
    document.querySelectorAll('.score-ring-svg').forEach(svg => {
        const circle = svg.querySelector('.score-ring-progress');
        if (!circle) return;
        const score = parseFloat(svg.dataset.score || 0);
        const r = parseFloat(circle.getAttribute('r'));
        const circumference = 2 * Math.PI * r;
        circle.style.strokeDasharray = circumference;
        circle.style.strokeDashoffset = circumference;
        setTimeout(() => {
            circle.style.transition = 'stroke-dashoffset 1s cubic-bezier(0.4,0,0.2,1)';
            circle.style.strokeDashoffset = circumference - (score / 100) * circumference;
        }, 200);
    });
}

/* ── MOBILE NAV ─────────────────────────────────────────────────── */
function initMobileNav() {
    const toggle = document.getElementById('nav-toggle');
    const sidebar = document.querySelector('.los-sidebar');
    const overlay = document.getElementById('nav-overlay');

    function openNav() {
        sidebar?.classList.add('open');
        overlay?.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeNav() {
        sidebar?.classList.remove('open');
        overlay?.classList.remove('active');
        document.body.style.overflow = '';
    }

    toggle?.addEventListener('click', openNav);
    overlay?.addEventListener('click', closeNav);
}

/* ── FORM ENHANCEMENTS ───────────────────────────────────────────── */
function initFormEnhancements() {
    // Auto-resize textareas
    document.querySelectorAll('textarea.form-control').forEach(ta => {
        function resize() {
            ta.style.height = 'auto';
            ta.style.height = Math.max(80, ta.scrollHeight) + 'px';
        }
        ta.addEventListener('input', resize);
    });

    // Date inputs — default to today when empty
    document.querySelectorAll('input[type="date"][data-default-today]').forEach(input => {
        if (!input.value) {
            input.value = new Date().toISOString().split('T')[0];
        }
    });
}

/* ── CONFIRM BEFORE DELETE ───────────────────────────────────────── */
function initConfirmForms() {
    document.querySelectorAll('[data-confirm]').forEach(el => {
        el.addEventListener('click', e => {
            const msg = el.dataset.confirm || 'Are you sure?';
            if (!confirm(msg)) e.preventDefault();
        });
    });
}

/* ── SCORE COLOR HELPER ──────────────────────────────────────────── */
function scoreColor(score) {
    if (score >= 75) return 'var(--green)';
    if (score >= 45) return 'var(--amber)';
    return 'var(--rose)';
}

/* ── SCORE RING RENDER ───────────────────────────────────────────── */
function buildScoreRing(score, size = 100, strokeWidth = 8) {
    const r = (size / 2) - (strokeWidth / 2);
    const c = 2 * Math.PI * r;
    const color = scoreColor(score);
    return `
    <svg class="score-ring-svg" data-score="${score}"
         width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
      <circle cx="${size / 2}" cy="${size / 2}" r="${r}"
              fill="none" stroke="var(--surface-3)" stroke-width="${strokeWidth}"/>
      <circle class="score-ring-progress" cx="${size / 2}" cy="${size / 2}" r="${r}"
              fill="none" stroke="${color}" stroke-width="${strokeWidth}"
              stroke-linecap="round"
              transform="rotate(-90 ${size / 2} ${size / 2})"/>
    </svg>`;
}

/* ── INJECT RINGS DYNAMICALLY ────────────────────────────────────── */
document.querySelectorAll('[data-ring-score]').forEach(wrap => {
    const score = parseFloat(wrap.dataset.ringScore);
    const size = parseInt(wrap.dataset.ringSize || 90);
    wrap.innerHTML = buildScoreRing(score, size) + wrap.innerHTML;
    initScoreRings();
});