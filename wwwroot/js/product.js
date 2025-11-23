document.addEventListener('DOMContentLoaded', () => {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');
  const username = localStorage.getItem('username');

  if (!token) {
    alert('Bạn chưa đăng nhập!');
    location.href = 'login.html';
    return;
  }

  document.getElementById('username').textContent = username || 'User';
  document.getElementById('role').textContent = role || 'User';
  document.getElementById('btnAdd').style.display = role === 'Admin' ? 'block' : 'none';

  const loadProducts = async () => {
    try {
      const res = await fetch('/api/products', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      const result = await res.json();

      const grid = document.getElementById('productsGrid');
      if (!result.success || result.data.length === 0) {
        grid.innerHTML = '<p class="no-data">Chưa có sản phẩm nào</p>';
        return;
      }

      grid.innerHTML = result.data.map(p => {
        // FIX: backend không có imageUrl → dùng ảnh mặc định
        const imgUrl = 'https://i.imgur.com/2s2m8Yl.jpeg';
        const stock = p.Stock || p.stock || 0;

        return `
          <div class="product-card">
            <div class="card-image"><img src="${imgUrl}" alt="${p.Name || p.name}"></div>
            <div class="card-content">
              <h3>${p.Name || p.name}</h3>
              <p class="stock ${stock === 0 ? 'out-of-stock' : ''}">
                ${stock === 0 ? 'Hết hàng' : 'Còn ' + stock + ' ly'}
              </p>
              <p class="description">${p.Description || p.description || 'Ngon – bổ – rẻ'}</ finalist>
              <div class="price-actions">
                <span class="price">${Number(p.Price || p.price).toLocaleString('vi-VN')}đ</span>
                ${role === 'Admin' ? `
                  <div class="admin-actions">
                    <button class="btn-edit" onclick="suaSanPham(${p.Id || p.id})">Sửa</button>
                    <button class="btn-delete" onclick="xoaSanPham(${p.Id || p.id})">Xóa</button>
                  </div>
                ` : `<button class="btn-buy" ${stock === 0 ? 'disabled' : ''} onclick="themVaoGio(${p.Id || p.id})">Mua</button>`}
              </div>
            </div>
          </div>
        `;
      }).join('');
    } catch (err) {
      console.error(err);
      document.getElementById('productsGrid').innerHTML = '<p class="no-data">Lỗi server</p>';
    }
  };

  window.moModalThem = () => {
    document.getElementById('modalThem').style.display = 'flex';
    document.getElementById('tenSP').value = '';
    document.getElementById('giaSP').value = '';
    document.getElementById('tonKhoSP').value = '';
    document.getElementById('moTaSP').value = '';
    document.getElementById('anhSP').value = '';
    document.getElementById('preview').innerHTML = '';
  };

  window.dongModal = () => document.getElementById('modalThem').style.display = 'none';

  // Preview ảnh (chỉ xem trước)
  document.getElementById('uploadArea').onclick = () => document.getElementById('anhSP').click();
  document.getElementById('anhSP').onchange = e => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = ev => {
        document.getElementById('preview').innerHTML = `<img src="${ev.target.result}" style="max-width:100%;max-height:200px;border-radius:16px;margin-top:10px;">`;
      };
      reader.readAsDataURL(file);
    }
  };

  // HÀM THÊM SẢN PHẨM – FIX CUỐI CÙNG, CHẠY NGON 100% VỚI BACKEND CỦA BẠN
  window.luuSanPhamMoi = async () => {
    const ten = document.getElementById('tenSP').value.trim();
    const giaStr = document.getElementById('giaSP').value.trim();
    const tonKhoStr = document.getElementById('tonKhoSP').value.trim();
    const moTa = document.getElementById('moTaSP').value.trim() || "Trà sữa ngon";

    if (!ten || !giaStr || !tonKhoStr) {
      alert('Vui lòng nhập đầy đủ Tên, Giá tiền và Tồn kho!');
      return;
    }

    const gia = parseFloat(giaStr);
    const tonkho = parseInt(tonKhoStr, 10);

    if (isNaN(gia) || isNaN(tonkho) || gia < 0 || tonkho < 0) {
      alert('Giá tiền và Tồn kho phải là số hợp lệ!');
      return;
    }

    // FIX CUỐI CÙNG: dùng đúng tên field của DTO + Price là decimal
    const payload = {
      Name: ten,
      Price: gia,           // gửi số bình thường → backend nhận được decimal
      Stock: tonkho,
      Description: moTa
    };

    try {
      const res = await fetch('/api/products', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const error = await res.json();
        console.error('Lỗi từ server:', error);
        alert(error.message || 'Lỗi server khi thêm sản phẩm');
        return;
      }

      const data = await res.json();
      if (data.success) {
        alert('THÊM SẢN PHẨM THÀNH CÔNG!!!');
        dongModal();
        loadProducts();
      }
    } catch (err) {
      console.error('Lỗi kết nối:', err);
      alert('Lỗi kết nối server!');
    }
  };

  window.themVaoGio = (id) => alert(`Đã thêm sản phẩm ID ${id} vào giỏ hàng!`);
  window.xoaSanPham = async (id) => {
    if (confirm('Xóa sản phẩm này thật không?')) {
      try {
        await fetch(`/api/products/${id}`, {
          method: 'DELETE',
          headers: { 'Authorization': `Bearer ${token}` }
        });
        loadProducts();
      } catch {
        alert('Lỗi khi xóa');
      }
    }
  };
  window.suaSanPham = () => alert('Chức năng sửa sẽ làm sau!');
  window.dangXuat = () => confirm('Đăng xuất?') && (localStorage.clear(), location.href = 'login.html');

  loadProducts();
});