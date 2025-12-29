// Site.js - E-commerce Dropshipping

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 5000);
    });
});

// Quantity input handlers
function updateQuantity(produitId, change) {
    const input = document.querySelector(`input[data-produit-id="${produitId}"]`);
    if (input) {
        let newValue = parseInt(input.value) + change;
        const max = parseInt(input.max) || 999;
        const min = parseInt(input.min) || 1;
        
        if (newValue >= min && newValue <= max) {
            input.value = newValue;
            input.form.submit();
        }
    }
}

// Search autocomplete (placeholder for future implementation)
const searchInput = document.querySelector('.search-form input');
if (searchInput) {
    let debounceTimer;
    searchInput.addEventListener('input', function() {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function() {
            // Could implement AJAX search here
        }, 300);
    });
}

// Add to cart animation
function addToCartAnimation(button) {
    button.innerHTML = '<i class="fas fa-check"></i> Ajouté !';
    button.classList.add('btn-success');
    button.classList.remove('btn-primary');
    
    setTimeout(function() {
        button.innerHTML = '<i class="fas fa-cart-plus me-1"></i> Ajouter au panier';
        button.classList.remove('btn-success');
        button.classList.add('btn-primary');
    }, 2000);
}

// Confirm delete
function confirmDelete(message) {
    return confirm(message || 'Êtes-vous sûr de vouloir supprimer cet élément ?');
}

// Mobile admin sidebar toggle
const adminSidebarToggle = document.getElementById('adminSidebarToggle');
if (adminSidebarToggle) {
    adminSidebarToggle.addEventListener('click', function() {
        document.querySelector('.admin-sidebar').classList.toggle('show');
    });
}

// Image lazy loading fallback
document.addEventListener('DOMContentLoaded', function() {
    const images = document.querySelectorAll('img[data-src]');
    images.forEach(function(img) {
        img.src = img.dataset.src;
    });
});

// Form validation styling
document.addEventListener('DOMContentLoaded', function() {
    const forms = document.querySelectorAll('form');
    forms.forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
});
