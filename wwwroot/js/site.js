// NedAcademyWebsite Foundational Client-side Scripts

document.addEventListener('DOMContentLoaded', () => {
    // 1. Initialize Sticky Header scroll behavior
    const header = document.getElementById('mainHeader');
    
    function checkScroll() {
        if (window.scrollY > 20) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    }
    
    if (header) {
        checkScroll();
        window.addEventListener('scroll', checkScroll, { passive: true });
    }

    // 2. Mobile Toggler & Mega-Dropdown Actions
    const mobileMenuToggler = document.getElementById('mobileMenuToggler');
    const navMenu = document.getElementById('navMenu');

    if (mobileMenuToggler && navMenu) {
        mobileMenuToggler.addEventListener('click', (e) => {
            e.stopPropagation();
            const isActive = navMenu.classList.toggle('active');
            mobileMenuToggler.setAttribute('aria-expanded', isActive);
            
            const icon = mobileMenuToggler.querySelector('i');
            if (icon) {
                icon.classList.toggle('fa-bars-staggered', !isActive);
                icon.classList.toggle('fa-xmark', isActive);
            }
        });

        // Mobile Mega Dropdown Accordion Toggle
        const megaDropdowns = navMenu.querySelectorAll('.mega-dropdown > a');
        megaDropdowns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                if (window.innerWidth < 992) {
                    e.preventDefault();
                    const parent = btn.parentElement;
                    parent.classList.toggle('active');
                }
            });
        });

        // Close mobile drawer on outside click
        document.addEventListener('click', (e) => {
            if (!navMenu.contains(e.target) && !mobileMenuToggler.contains(e.target)) {
                if (navMenu.classList.contains('active')) {
                    navMenu.classList.remove('active');
                    mobileMenuToggler.setAttribute('aria-expanded', 'false');
                    const icon = mobileMenuToggler.querySelector('i');
                    if (icon) {
                        icon.classList.add('fa-bars-staggered');
                        icon.classList.remove('fa-xmark');
                    }
                }
            }
        });
    }

    // 3. Newsletter Submission (SweetAlert2 Integration)
    const newsletterForm = document.getElementById('newsletterForm');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const emailInput = newsletterForm.querySelector('input[type="email"]');
            if (emailInput && emailInput.value.trim() !== '') {
                Swal.fire({
                    title: 'Access Configured!',
                    text: `Newsletter subscription registered for: ${emailInput.value}`,
                    icon: 'success',
                    background: '#0b1329',
                    color: '#f8fafc',
                    confirmButtonColor: '#0077ff',
                    confirmButtonText: 'Great',
                    customClass: {
                        popup: 'glass-card border-0'
                    }
                });
                newsletterForm.reset();
            }
        });
    }

    // 3.5 Theme Toggle Button Logic
    const themeToggleBtn = document.getElementById('themeToggleBtn');
    if (themeToggleBtn) {
        const updateIcon = () => {
            const icon = themeToggleBtn.querySelector('i');
            if (icon) {
                if (document.body.classList.contains('light-theme')) {
                    icon.className = 'fa-solid fa-sun text-warning';
                } else {
                    icon.className = 'fa-solid fa-moon';
                }
            }
        };
        
        updateIcon();

        themeToggleBtn.addEventListener('click', () => {
            const body = document.body;
            body.classList.toggle('light-theme');
            document.documentElement.classList.toggle('light-theme');
            const isLight = body.classList.contains('light-theme');
            localStorage.setItem('theme', isLight ? 'light' : 'dark');
            updateIcon();
            
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: `${isLight ? 'Light' : 'Dark'} mode activated`,
                showConfirmButton: false,
                timer: 1500,
                background: isLight ? '#f8fafc' : '#0b1329',
                color: isLight ? '#0f172a' : '#f8fafc',
                customClass: {
                    popup: 'glass-card border-0'
                }
            });
        });
    }

    // Fast Entrance Trigger
    triggerEntranceAnimations();
});

// 4. Instant Preloader Dismissal
function dismissLoader() {
    const preloader = document.getElementById('preloader');
    if (preloader && preloader.style.display !== 'none') {
        preloader.style.opacity = '0';
        preloader.style.transition = 'opacity 0.15s ease';
        setTimeout(() => {
            preloader.style.display = 'none';
            preloader.style.visibility = 'hidden';
        }, 150);
    }
}

document.addEventListener('DOMContentLoaded', dismissLoader);
window.addEventListener('load', dismissLoader);

function triggerEntranceAnimations() {
    if (typeof gsap !== 'undefined') {
        if (typeof ScrollTrigger !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);
        }

        gsap.from('#mainHeader', {
            y: -50,
            opacity: 0,
            duration: 0.5,
            ease: 'power2.out'
        });

        gsap.utils.toArray('.reveal-up').forEach((el) => {
            gsap.from(el, {
                scrollTrigger: {
                    trigger: el,
                    start: 'top 90%',
                    toggleActions: 'play none none none'
                },
                y: 30,
                opacity: 0,
                duration: 0.6,
                ease: 'power2.out'
            });
        });
    }
}
