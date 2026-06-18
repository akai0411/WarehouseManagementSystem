using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Domain.Entities;

namespace WarehouseManagementSystem.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Product>> GetAllPagedAsync(int page, int pageSize)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await
                _context.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(); ;
        }
        public async Task DeleteAsync(Product product)
        {
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
