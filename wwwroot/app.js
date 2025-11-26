/**
 * app.js - File JavaScript chính cho ứng dụng
 * Xử lý xác thực, điều hướng, và các chức năng chung
 */

// API Base URL
const API_URL = 'http://localhost:5162/api';

/**
 * Kiểm tra xác thực - Redirect nếu chưa đăng nhập hoặc không đủ quyền
 * @param {string} requiredRole - Vai trò cần thiết ('Admin', 'User', hoặc null)
 */
function checkAuth(requiredRole = null) {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (!token) {
        // Chưa đăng nhập
        window.location.href = 'login.html';
        return;
    }

    if (requiredRole && role !== requiredRole) {
        // Không đủ quyền
        alert('Bạn không có quyền truy cập trang này');
        window.location.href = 'index.html';
        return;
    }
}

/**
 * Cập nhật menu điều hướng dựa trên trạng thái đăng nhập
 */
function updateNavigation() {
     const token = localStorage.getItem('token');
     const role = localStorage.getItem('role');
     const username = localStorage.getItem('username');
 
     const navLogin = document.getElementById('navLogin');
     const logoutBtn = document.getElementById('logoutBtn');
     const navProducts = document.getElementById('navProducts');
     const navAdmin = document.getElementById('navAdmin');
     const navAdminOrders = document.getElementById('navAdminOrders');
     const navMyOrders = document.getElementById('navMyOrders');
     const navCart = document.getElementById('navCart');
     
     if (token) {
         // Đã đăng nhập
         if (navLogin) navLogin.style.display = 'none';
         if (logoutBtn) {
             logoutBtn.style.display = 'inline-block';
             // Xóa old listener trước khi thêm mới
             logoutBtn.onclick = logout;
         }
     
         if (role === 'Admin' && navAdmin) {
             navAdmin.style.display = 'inline-block';
         } else if (role === 'User' && navProducts) {
             navProducts.style.display = 'inline-block';
         }
         
         // Hiển thị Quản Lý Đơn Hàng cho Admin
         if (role === 'Admin' && navAdminOrders) {
             navAdminOrders.style.display = 'inline-block';
         }
         
         // Hiển thị Đơn Hàng Của Tôi cho User
         if (role === 'User' && navMyOrders) {
             navMyOrders.style.display = 'inline-block';
         }
         
         // Hiển thị giỏ hàng cho User
         if (role === 'User' && navCart) {
             navCart.style.display = 'inline-block';
         }
     } else {
         // Chưa đăng nhập
         if (navLogin) navLogin.style.display = 'inline-block';
         if (logoutBtn) {
             logoutBtn.style.display = 'none';
             logoutBtn.onclick = null;
         }
         if (navProducts) navProducts.style.display = 'none';
         if (navAdmin) navAdmin.style.display = 'none';
         if (navAdminOrders) navAdminOrders.style.display = 'none';
         if (navMyOrders) navMyOrders.style.display = 'none';
         if (navCart) navCart.style.display = 'none';
     }
 
     // Cập nhật số lượng giỏ hàng
     updateCartCount();
 }

/**
 * Xử lý đăng xuất
 */
function logout() {
    if (confirm('Bạn chắc chắn muốn đăng xuất?')) {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('username');
        localStorage.removeItem('customerId');
        // Giữ giỏ hàng nhưng xóa nó sau khi người dùng đăng xuất
        // localStorage.removeItem('cart');
        window.location.href = 'login.html';
    }
}

/**
 * Cập nhật số lượng sản phẩm trong giỏ hàng
 */
function updateCartCount() {
    const cart = JSON.parse(localStorage.getItem('cart')) || [];
    const count = cart.reduce((sum, item) => sum + item.quantity, 0);
    
    const cartCountElements = document.querySelectorAll('#cartCount');
    cartCountElements.forEach(el => {
        el.textContent = count;
    });
}

/**
 * Gọi API với xác thực
 * @param {string} endpoint - Đường dẫn endpoint (ví dụ: '/products')
 * @param {object} options - Tùy chọn fetch
 * @returns {Promise} Kết quả API
 */
async function apiFetch(endpoint, options = {}) {
    const token = localStorage.getItem('token');
    const defaultHeaders = {
        'Content-Type': 'application/json'
    };

    if (token) {
        defaultHeaders['Authorization'] = `Bearer ${token}`;
    }

    const fetchOptions = {
        ...options,
        headers: {
            ...defaultHeaders,
            ...(options.headers || {})
        }
    };

    const response = await fetch(`${API_URL}${endpoint}`, fetchOptions);

    // Nếu token không hợp lệ, đăng xuất
    if (response.status === 401) {
        logout();
        throw new Error('Phiên làm việc đã hết hạn');
    }

    return response;
}

/**
 * Định dạng tiền tệ VNĐ
 * @param {number} amount - Số tiền
 * @returns {string} Chuỗi định dạng
 */
function formatCurrency(amount) {
    return amount.toLocaleString('vi-VN');
}

/**
 * Định dạng ngày tháng
 * @param {string} dateString - Chuỗi ngày
 * @returns {string} Ngày định dạng
 */
function formatDate(dateString) {
    return new Date(dateString).toLocaleString('vi-VN');
}

/**
 * Hiển thị thông báo
 * @param {string} message - Nội dung thông báo
 * @param {string} type - Loại: 'success', 'error', 'info'
 * @param {number} duration - Thời gian hiển thị (ms)
 */
function showNotification(message, type = 'info', duration = 3000) {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        background-color: ${type === 'success' ? '#d4edda' : type === 'error' ? '#f8d7da' : '#d1ecf1'};
        color: ${type === 'success' ? '#155724' : type === 'error' ? '#721c24' : '#0c5460'};
        border-radius: 4px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        z-index: 2000;
        border-left: 4px solid ${type === 'success' ? '#28a745' : type === 'error' ? '#e74c3c' : '#3498db'};
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, duration);
}

/**
 * Kiểm tra JWT token còn hợp lệ không
 */
function isTokenValid() {
    const token = localStorage.getItem('token');
    return token !== null && token !== '';
}

// Lắng nghe sự kiện khi tải trang
document.addEventListener('DOMContentLoaded', function() {
    // Cập nhật số lượng giỏ hàng khi load
    updateCartCount();
    
    // Setup logout button
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.onclick = function(e) {
            e.preventDefault();
            logout();
        };
    }
});

// Cập nhật số lượng giỏ hàng khi lưu trữ thay đổi
window.addEventListener('storage', function(e) {
    if (e.key === 'cart') {
        updateCartCount();
    }
});
