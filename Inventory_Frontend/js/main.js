
document.addEventListener('DOMContentLoaded', function() {
    // initialize modules
    UI.init();
    Auth.init();
    
    console.log('7-Eleven Inventory System');
});

// global error handler
window.addEventListener('error', function(e) {
    console.error('Global error:', e.error);
});

// handle unhandled promise rejections
window.addEventListener('unhandledrejection', function(e) {
    console.error('Unhandled promise rejection:', e.reason);
});
