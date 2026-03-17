using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Product.Domain.Repositories;
using Product.Infrastructure.Persistence;
using Product.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Features.Products.Commands.UpdateProduct;
using Product.Application.Features.Products.Queries.GetProducts;
using Product.Application.Interfaces;
using Product.Infrastructure.Caching;
using Product.Application.Interfaces;
using Product.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Ayarları
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Sadece token gir. Bearer yazmana gerek yok."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CQRS Handler Kayıtları
builder.Services.AddScoped<CreateProductCommandHandler>();
builder.Services.AddScoped<UpdateProductCommandHandler>();
builder.Services.AddScoped<GetProductsQueryHandler>();

// Veritabanı ve Repository Kayıtları
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDb")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// -------------------------------------------------------------------
// 👇 REDIS VE CACHE SERVİSİ KODLARINI BURAYA EKLEDİK 👇
// -------------------------------------------------------------------
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
});
builder.Services.AddScoped<IProductCacheService, ProductCacheService>();
// -------------------------------------------------------------------

// JWT ve Authentication Ayarları
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddHttpClient<ILogServiceClient, LogServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:LogServiceBaseUrl"]!);
});

builder.Services.AddAuthorization();

// --- UYGULAMA İNŞA EDİLİYOR ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/ping", () => Results.Ok("product pong"));

app.Run();