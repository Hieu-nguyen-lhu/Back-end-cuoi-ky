using Microsoft.EntityFrameworkCore;
using back_end_cuoi_ky.Data;
using back_end_cuoi_ky.Dtos;
using back_end_cuoi_ky.Models;

namespace back_end_cuoi_ky.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return customers.Select(c => MapToDto(c));
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            return customer == null ? null : MapToDto(customer);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            // Kiểm tra email đã tồn tại
            if (await EmailExistsAsync(dto.Email))
            {
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
            }

            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return null;

            // ✅ Partial update: only update fields that are provided (not null)
            if (!string.IsNullOrWhiteSpace(dto.Name))
                customer.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                // Kiểm tra email đã tồn tại (trừ chính khách hàng này)
                if (await EmailExistsAsync(dto.Email, id))
                {
                    throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
                }
                customer.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                customer.Phone = dto.Phone;

            if (dto.Address != null)
                customer.Address = dto.Address;

            customer.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email && (excludeId == null || c.Id != excludeId));
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}