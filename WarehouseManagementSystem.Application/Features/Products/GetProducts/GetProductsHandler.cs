using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Common.Mapping;
using WarehouseManagementSystem.Application.Common.Responses;
using WarehouseManagementSystem.Application.Features.Products.Common;

namespace WarehouseManagementSystem.Application.Features.Products.GetProducts
{
    public class GetProductsHandler
    {
        private readonly IProductRepository _repository;

        public GetProductsHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<ProductDto>> Handle(GetProductsQuery query)
        {
            var (products, totalCount) = await _repository.GetAllAsync(query);

            var items = products
                .Select(ProductMapper.ToDto)
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<ProductDto>
            {
                Items = items,
                Page = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
}