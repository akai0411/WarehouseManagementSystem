using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Products.GetProducts;
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
        public async Task<(List<Product> Items, int TotalCount)> GetAllAsync(GetProductsQuery query)
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query.Name))
                products = products.Where(p => p.Name.Contains(query.Name));

            if (!string.IsNullOrWhiteSpace(query.SKU))
                products = products.Where(p => p.SKU.Contains(query.SKU));

            
            var totalCount = await products.CountAsync();

            products = query.SortBy?.ToLower() switch
            {
                "name" => query.Descending
                    ? products.OrderByDescending(p => p.Name)
                    : products.OrderBy(p => p.Name),

                "price" => query.Descending
                    ? products.OrderByDescending(p => p.Price)
                    : products.OrderBy(p => p.Price),

                "sku" => query.Descending
                    ? products.OrderByDescending(p => p.SKU)
                    : products.OrderBy(p => p.SKU),

                _ => products.OrderBy(p => p.Name)
            };

            var items = await products
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
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

        public async Task<Product?> GetBySkuAsync(string sku)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == sku && !p.IsDeleted);
        }
    }
}
