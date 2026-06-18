using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Application.Features.Products.DeleteProduct;
using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Application.Features.Products.UpdateProduct;
using WarehouseManagementSystem.Application.Features.Products.GetProductById;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Infrastructure.Persistence;
using WarehouseManagementSystem.Infrastructure.Persistence.Repositories;
using WarehouseManagementSystem.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

#region Controllers + Swagger

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region DbContext

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

#endregion

#region Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
#endregion

#region Handlers (Application Layer)

builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetProductsHandler>();
builder.Services.AddScoped<GetProductByIdHandler>();
builder.Services.AddScoped<UpdateProductHandler>();
builder.Services.AddScoped<DeleteProductHandler>();

#endregion

#region FluentValidation

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

#endregion

var app = builder.Build();

#region Middleware

app.UseMiddleware<ExceptionMiddleware>();

#endregion

#region Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
