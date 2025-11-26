# Quản Lý Bán Hàng Online

Ứng dụng web quản lý bán hàng với chức năng quản lý sản phẩm, đơn hàng, khách hàng và người dùng.

## Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 6.0+, Entity Framework Core, SQL Server
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **Authentication**: JWT Token

## Cấu Trúc Dự Án

```
Back-end-cuoi-ky/
├── Controllers/          # API Controllers
├── Models/               # Database Models
├── Services/             # Business Logic
├── Dtos/                # Data Transfer Objects
├── Data/                # Database Context
├── Migrations/          # EF Core Migrations
├── wwwroot/             # Static Files (HTML, CSS, JS)
├── appsettings.json     # Configuration
└── Program.cs           # Startup Configuration
```

## Hướng Dẫn Chạy Backend

### Yêu Cầu
- .NET 6.0+ SDK
- SQL Server (hoặc LocalDB)
- Visual Studio 2022 / Visual Studio Code

### Bước 1: Cài Đặt Dependencies
```bash
dotnet restore
```

### Bước 2: Cấu Hình Database
Sửa connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=back_end_cuoi_ky;Trusted_Connection=true;"
  }
}
```

### Bước 3: Chạy Migrations
```powershell
# Trong Package Manager Console
Add-Migration InitialCreate
Update-Database
```

Hoặc dùng dotnet CLI:
```bash
dotnet ef database update
```

### Bước 4: Chạy Backend
```bash
dotnet run
```

Backend sẽ chạy tại: `http://localhost:5162/Swagger/index.html`

## Hướng Dẫn Chạy Frontend

Frontend là các file HTML tĩnh nằm trong thư mục `wwwroot/`.

```bash
dotnet run
```
Frontend sẽ được server tự động khi backend chạy:
- Truy cập: `http://localhost:5162/login.html`
- Các file tĩnh sẽ được phục vụ từ thư mục `wwwroot/`



## Các Trang Frontend

| Trang | URL | Mô Tả |
|-------|-----|-------|
| Trang Chủ | `index.html` | Dashboard, sản phẩm nổi bật |
| Đăng Nhập | `login.html` | Xác thực người dùng |
| Đăng Ký | `register.html` | Tạo tài khoản mới |
| Danh Sách Sản Phẩm | `products.html` | Xem và mua sản phẩm |
| Giỏ Hàng | `cart.html` | Xem & thanh toán |
| Đơn Hàng Của Tôi | `my-orders.html` | Lịch sử đơn hàng (User) |
| Quản Lý Sản Phẩm | `admin-products.html` | CRUD sản phẩm (Admin) |
| Quản Lý Đơn Hàng | `admin-orders.html` | Quản lý & xóa đơn hàng (Admin) |

## API Endpoints

### Authentication
```
POST   /api/auth/login          # Đăng nhập
POST   /api/auth/register       # Đăng ký
```

### Products
```
GET    /api/products            # Lấy tất cả sản phẩm
GET    /api/products/{id}       # Lấy sản phẩm theo ID
POST   /api/products            # Tạo sản phẩm (Admin)
PUT    /api/products/{id}       # Cập nhật sản phẩm (Admin)
DELETE /api/products/{id}       # Xóa sản phẩm (Admin)
```

### Orders
```
GET    /api/orders              # Lấy tất cả đơn hàng (Admin)
GET    /api/orders/{id}         # Lấy chi tiết đơn hàng
GET    /api/orders/my           # Lấy đơn hàng của user hiện tại
POST   /api/orders              # Tạo đơn hàng mới
PUT    /api/orders/{id}         # Cập nhật trạng thái đơn hàng (Admin)
DELETE /api/orders/{id}         # Xóa đơn hàng (Admin)
```

### Customers
```
GET    /api/customers           # Lấy tất cả khách hàng
GET    /api/customers/{id}      # Lấy khách hàng theo ID
POST   /api/customers           # Tạo khách hàng
PUT    /api/customers/{id}      # Cập nhật khách hàng
DELETE /api/customers/{id}      # Xóa khách hàng
```


## Các Chức Năng Chính

### Cho User
- ✅ Đăng ký & Đăng nhập
- ✅ Xem danh sách sản phẩm
- ✅ Thêm sản phẩm vào giỏ hàng
- ✅ Thanh toán & tạo đơn hàng
- ✅ Xem lịch sử đơn hàng
- ✅ Xem chi tiết đơn hàng (số điện thoại, địa chỉ giao hàng)

### Cho Admin
- ✅ Quản lý sản phẩm (CRUD)
- ✅ Quản lý đơn hàng (xem, cập nhật trạng thái, xóa)
- ✅ Xem thống kê (tổng sản phẩm, tổng đơn hàng, đơn hàng đã giao)

## Database Schema

### Customers
```
- Id (PK)
- Name
- Email
- Phone
- Address
- CreatedAt
- UpdatedAt
```

### Products
```
- Id (PK)
- Name
- Description
- Price
- Stock
- ImageUrl
- CreatedAt
- UpdatedAt
```

### Orders
```
- Id (PK)
- CustomerId (FK)
- OrderDate
- Status (Pending, Confirmed, Shipped, Delivered, Cancelled)
- TotalAmount
- PhoneNumber
- DeliveryAddress
- CreatedAt
- UpdatedAt
```

### OrderDetails
```
- Id (PK)
- OrderId (FK)
- ProductId (FK - Nullable)
- Quantity
- UnitPrice
- Subtotal
- CreatedAt
```

### Users
```
- Id (PK)
- Username
- PasswordHash
- CustomerId (FK - Nullable)
- Role (Admin, User)
- CreatedAt
```

## Lưu Ý Kỹ Thuật

1. **JWT Authentication**: Token được lưu trong localStorage, tự động gửi trong header `Authorization: Bearer {token}`

2. **CORS**: Backend được cấu hình cho phép requests từ frontend

3. **DeleteBehavior**: 
   - Order → OrderDetail: `Cascade` (xóa order sẽ xóa tất cả order details)
   - OrderDetail → Product: `SetNull` (xóa product sẽ set ProductId = null)

4. **Validation**: Dữ liệu được validate ở cả Frontend và Backend

5. **Error Handling**: API trả về message lỗi chi tiết, frontend hiển thị alert

## Troubleshooting

### Backend không kết nối được DB
- Kiểm tra connection string trong `appsettings.json`
- Chắc chắn SQL Server đang chạy
- Kiểm tra firewall settings

### CORS Error
- Kiểm tra API_URL trong `wwwroot/app.js` phù hợp với backend URL
- Xác nhận backend được cấu hình cho phép origin

### Frontend không load
- Xóa cache browser: `Ctrl+Shift+Delete`
- Xóa localStorage: F12 → Application → Clear All
- Reload trang: `Ctrl+R`

## Liên Hệ & Support

Nếu gặp lỗi, vui lòng:
1. Kiểm tra console browser (F12)
2. Kiểm tra Network tab để xem API requests
3. Kiểm tra backend logs
4. Tạo issue với mô tả chi tiết

---

**Tác Giả**: Nguyễn Đức Hiệu / Ngô Tấn Lộc / Lê Minh Luân 
**Ngày Tạo**: 2025-11-26  
**Version**: 1.0.0
