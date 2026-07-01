using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;
using WarehouseManagementSystem.Api.Data;
using WarehouseManagementSystem.Application.Common.Interface;
using WarehouseManagementSystem.Application.Features.Auth.AssignRole;
using WarehouseManagementSystem.Application.Features.Auth.Login;
using WarehouseManagementSystem.Application.Features.Auth.Register;
using WarehouseManagementSystem.Application.Features.Inventory.CreateInventory;
using WarehouseManagementSystem.Application.Features.Inventory.DeleteInventory;
using WarehouseManagementSystem.Application.Features.Inventory.GetInventory;
using WarehouseManagementSystem.Application.Features.Inventory.GetInventoryById;
using WarehouseManagementSystem.Application.Features.Locations.CreateLocation;
using WarehouseManagementSystem.Application.Features.Locations.DeleteLocation;
using WarehouseManagementSystem.Application.Features.Locations.GetLocationById;
using WarehouseManagementSystem.Application.Features.Locations.GetLocations;
using WarehouseManagementSystem.Application.Features.Locations.UpdateLocation;
using WarehouseManagementSystem.Application.Features.Products.CreateProduct;
using WarehouseManagementSystem.Application.Features.Products.DeleteProduct;
using WarehouseManagementSystem.Application.Features.Products.GetProductById;
using WarehouseManagementSystem.Application.Features.Products.GetProducts;
using WarehouseManagementSystem.Application.Features.Products.UpdateProduct;
using WarehouseManagementSystem.Application.Features.StockMovements.CreateStockMovement;
using WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovementById;
using WarehouseManagementSystem.Application.Features.StockMovements.GetStockMovements;
using WarehouseManagementSystem.Application.Features.Warehouses.CreateWarehouse;
using WarehouseManagementSystem.Application.Features.Warehouses.DeleteWarehouse;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouseById;
using WarehouseManagementSystem.Application.Features.Warehouses.GetWarehouses;
using WarehouseManagementSystem.Application.Features.Warehouses.UpdateWarehouse;
using WarehouseManagementSystem.Application.Features.Zones.CreateZone;
using WarehouseManagementSystem.Application.Features.Zones.DeleteZone;
using WarehouseManagementSystem.Application.Features.Zones.GetZoneById;
using WarehouseManagementSystem.Application.Features.Zones.GetZones;
using WarehouseManagementSystem.Application.Features.Zones.UpdateZone;
using WarehouseManagementSystem.Infrastructure.Persistence;
using WarehouseManagementSystem.Infrastructure.Persistence.Repositories;
using WarehouseManagementSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

#region Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console()
      .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});
#endregion

#region Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Warehouse API",
        Version = "v1"
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // JWT AUTH
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in this format: Bearer {your_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
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
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
#endregion

#region Security
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService,
    WarehouseManagementSystem.Api.Services.CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
#endregion

#region Handlers 
//Product
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetProductsHandler>();
builder.Services.AddScoped<GetProductByIdHandler>();
builder.Services.AddScoped<UpdateProductHandler>();
builder.Services.AddScoped<DeleteProductHandler>();
//User
builder.Services.AddScoped<RegisterHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<AssignRoleHandler>();
//Warehouse
builder.Services.AddScoped<CreateWarehouseHandler>();
builder.Services.AddScoped<GetWarehousesHandler>();
builder.Services.AddScoped<GetWarehouseByIdHandler>();
builder.Services.AddScoped<UpdateWarehouseHandler>();
builder.Services.AddScoped<DeleteWarehouseHandler>();
// Zones
builder.Services.AddScoped<CreateZoneHandler>();
builder.Services.AddScoped<GetZonesHandler>();
builder.Services.AddScoped<GetZoneByIdHandler>();
builder.Services.AddScoped<UpdateZoneHandler>();
builder.Services.AddScoped<DeleteZoneHandler>();
// Locations
builder.Services.AddScoped<CreateLocationHandler>();
builder.Services.AddScoped<GetLocationsHandler>();
builder.Services.AddScoped<GetLocationByIdHandler>();
builder.Services.AddScoped<UpdateLocationHandler>();
builder.Services.AddScoped<DeleteLocationHandler>();
// Inventory
builder.Services.AddScoped<CreateInventoryHandler>();
builder.Services.AddScoped<GetInventoryHandler>();
builder.Services.AddScoped<GetInventoryByIdHandler>();
builder.Services.AddScoped<DeleteInventoryHandler>();
// Stock Movement
builder.Services.AddScoped<CreateStockMovementHandler>();
builder.Services.AddScoped<GetStockMovementsHandler>();
builder.Services.AddScoped<GetStockMovementByIdHandler>();
#endregion

#region FluentValidation
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
#endregion

#region Auth
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key missing");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

#region Migrations
// Applies any pending EF Core migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
#endregion

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#region Seeder
if (app.Environment.IsDevelopment())
{
    await DataSeeder.SeedAsync(app.Services);
}
#endregion

app.Run();
#endregion


