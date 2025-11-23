document.addEventListener('DOMContentLoaded', () => {
  const btnLogin = document.getElementById('btnLogin');
  const msg = document.getElementById('msg');

  const dangNhap = async () => {
    const username = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value;

    if (!username || !password) {
      showMessage('Vui lòng nhập đầy đủ!', 'error');
      return;
    }

    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });

      const data = await res.json();

      if (res.ok && data.success) {
        localStorage.setItem('token', data.data.token);
        localStorage.setItem('username', data.data.username);
        localStorage.setItem('role', data.data.role);

        showMessage(`Chào ${data.data.role} ${data.data.username}! Đang vào...`, 'success');
        
        setTimeout(() => {
          window.location.href = 'product.html';  // CHUYỂN QUA TRANG PRODUCT
        }, 1200);
      } else {
        showMessage(data.message || 'Sai tài khoản hoặc mật khẩu!', 'error');
      }
    } catch (err) {
      showMessage('Không kết nối được server!', 'error');
    }
  };

  const showMessage = (text, type) => {
    msg.textContent = text;
    msg.className = `message ${type}`;
    msg.style.display = 'block';
  };

  btnLogin.addEventListener('click', dangNhap);
  document.addEventListener('keypress', e => {
    if (e.key === 'Enter') dangNhap();
  });
});