// Site-wide JavaScript. Page-specific scripts (booking page, employee selection, etc.)
// live inside their own views in a @section Scripts block.

// Shows a simple full-page spinner whenever a <form> on the page is submitted, so the
// user gets feedback while the server processes the request (validation, database save, etc.).
document.addEventListener('submit', function (event) {
    // Only show the overlay if the browser is not going to block the submit
    // (e.g. a "confirm delete" dialog that the user cancelled).
    if (event.defaultPrevented) {
        return;
    }

    var overlay = document.getElementById('page-loading-overlay');
    if (overlay) {
        overlay.style.display = 'flex';
    }
});
