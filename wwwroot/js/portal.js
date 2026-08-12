/* ═══════════════════════════════════════════════════════════════════
   NED ACADEMY — ENTERPRISE APPLICANT PORTAL JAVASCRIPT
   Sidebar, Theme, Dropdowns, Keyboard Accessibility
   ═══════════════════════════════════════════════════════════════════ */

(function () {
    'use strict';

    // ──────── DOM Ready ────────
    document.addEventListener('DOMContentLoaded', function () {
        initSidebar();
        initThemeToggle();
        initProfileDropdown();
        initActiveNavHighlight();
        initPreloader();
        initMobileOverlay();
        initKeyboardAccessibility();
    });

    // ──────── SIDEBAR TOGGLE ────────
    function initSidebar() {
        var toggleBtn = document.getElementById('portalSidebarToggle');
        var body = document.body;
        var savedState = localStorage.getItem('portalSidebarCollapsed');

        // Restore saved state (desktop only)
        if (savedState === 'true' && window.innerWidth > 992) {
            body.classList.add('portal-sidebar-collapsed');
        }

        if (toggleBtn) {
            toggleBtn.addEventListener('click', function () {
                if (window.innerWidth <= 992) {
                    // Mobile: toggle overlay sidebar
                    var sidebar = document.querySelector('.portal-sidebar');
                    var overlay = document.querySelector('.portal-mobile-overlay');
                    if (sidebar) sidebar.classList.toggle('mobile-open');
                    if (overlay) overlay.classList.toggle('show');
                } else {
                    // Desktop: collapse sidebar
                    body.classList.toggle('portal-sidebar-collapsed');
                    localStorage.setItem('portalSidebarCollapsed', body.classList.contains('portal-sidebar-collapsed'));
                }
            });
        }
    }

    // ──────── MOBILE OVERLAY ────────
    function initMobileOverlay() {
        var overlay = document.querySelector('.portal-mobile-overlay');
        if (overlay) {
            overlay.addEventListener('click', function () {
                var sidebar = document.querySelector('.portal-sidebar');
                if (sidebar) sidebar.classList.remove('mobile-open');
                overlay.classList.remove('show');
            });
        }
    }

    // ──────── THEME TOGGLE ────────
    function initThemeToggle() {
        var themeBtn = document.getElementById('portalThemeToggle');
        var savedTheme = localStorage.getItem('portalTheme');

        if (savedTheme === 'light') {
            document.body.classList.add('portal-light');
            updateThemeIcon(true);
        }

        if (themeBtn) {
            themeBtn.addEventListener('click', function () {
                var isLight = document.body.classList.toggle('portal-light');
                localStorage.setItem('portalTheme', isLight ? 'light' : 'dark');
                updateThemeIcon(isLight);
            });
        }
    }

    function updateThemeIcon(isLight) {
        var icon = document.querySelector('#portalThemeToggle i');
        if (icon) {
            icon.className = isLight ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
        }
    }

    // ──────── PROFILE DROPDOWN ────────
    function initProfileDropdown() {
        var trigger = document.querySelector('.portal-profile-trigger');
        var menu = document.querySelector('.portal-profile-menu');

        if (trigger && menu) {
            trigger.addEventListener('click', function (e) {
                e.stopPropagation();
                menu.classList.toggle('show');
            });

            document.addEventListener('click', function (e) {
                if (!trigger.contains(e.target) && !menu.contains(e.target)) {
                    menu.classList.remove('show');
                }
            });
        }
    }

    // ──────── ACTIVE NAV HIGHLIGHT ────────
    function initActiveNavHighlight() {
        var currentPath = window.location.pathname.toLowerCase();
        var navItems = document.querySelectorAll('.portal-nav-item[href]');

        navItems.forEach(function (item) {
            var href = item.getAttribute('href');
            if (href) {
                var itemPath = href.toLowerCase();
                // Exact match or starts with (for sub-routes)
                if (currentPath === itemPath || (itemPath !== '/Admission/Dashboard' && currentPath.startsWith(itemPath))) {
                    item.classList.add('active');
                }
            }
        });
    }

    // ──────── PRELOADER ────────
    function initPreloader() {
        var preloader = document.getElementById('portalPreloader');
        if (preloader) {
            setTimeout(function () {
                preloader.classList.add('fade-out');
                setTimeout(function () {
                    preloader.style.display = 'none';
                }, 300);
            }, 200);
        }
    }

    // ──────── KEYBOARD ACCESSIBILITY ────────
    function initKeyboardAccessibility() {
        document.addEventListener('keydown', function (e) {
            // Escape: close mobile sidebar
            if (e.key === 'Escape') {
                var sidebar = document.querySelector('.portal-sidebar.mobile-open');
                var overlay = document.querySelector('.portal-mobile-overlay.show');
                if (sidebar) sidebar.classList.remove('mobile-open');
                if (overlay) overlay.classList.remove('show');

                // Close profile dropdown
                var menu = document.querySelector('.portal-profile-menu.show');
                if (menu) menu.classList.remove('show');
            }
        });
    }

    // ──────── PASSWORD STRENGTH METER ────────
    window.portalPasswordStrength = function (input) {
        var bars = document.querySelectorAll('.portal-password-bar');
        if (!bars.length) return;

        var val = input.value;
        var strength = 0;

        if (val.length >= 6) strength++;
        if (val.length >= 8) strength++;
        if (/[A-Z]/.test(val)) strength++;
        if (/[0-9]/.test(val)) strength++;
        if (/[^A-Za-z0-9]/.test(val)) strength++;

        bars.forEach(function (bar, idx) {
            bar.className = 'portal-password-bar';
            if (idx < strength) {
                if (strength <= 2) bar.classList.add('weak');
                else if (strength <= 3) bar.classList.add('medium');
                else bar.classList.add('filled');
            }
        });
    };

    // ──────── PROFILE COMPLETION RING ────────
    window.portalSetCompletionRing = function (percentage) {
        var ring = document.querySelector('.progress-ring');
        if (!ring) return;
        var radius = ring.getAttribute('r');
        var circumference = 2 * Math.PI * radius;
        ring.style.strokeDasharray = circumference;
        ring.style.strokeDashoffset = circumference - (percentage / 100) * circumference;
    };

})();
