// Site-wide JavaScript. Page-specific scripts (booking page, employee selection, etc.)
// live inside their own views in a @section Scripts block.

// Shows a dental logo spinner whenever a <form> on the page is submitted.
document.addEventListener('submit', function (event) {
    if (event.defaultPrevented) {
        return;
    }

    var overlay = document.getElementById('page-loading-overlay');
    if (overlay) {
        overlay.style.display = 'flex';
    }
});

// Stagger dashboard stat cards and list rows slightly for a polished entrance.
document.addEventListener('DOMContentLoaded', function () {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        return;
    }

    var cards = document.querySelectorAll('.stat-card');
    cards.forEach(function (card, index) {
        card.style.animation = 'fadeUp 0.55s cubic-bezier(0.22, 1, 0.36, 1) both';
        card.style.animationDelay = (0.08 + index * 0.07) + 's';
    });
});
