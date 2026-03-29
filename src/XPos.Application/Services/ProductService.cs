using XPos.Shared.DTOs;
using XPos.Application.Interfaces;
using XPos.Domain.Interfaces;
using XPos.Shared.Entities;

namespace XPos.Application.Services;

public class ProductService : IProductService
{
    private readonly IGenericRepository<Product> _productRepository;

    public ProductService(IGenericRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        // Manuel mapping (AutoMapper yerine basit tutmak için)
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            CategoryId = p.CategoryId,
            IsActive = p.IsActive
        });
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive
        };
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            ImageUrl = productDto.ImageUrl,
            CategoryId = productDto.CategoryId,
            IsActive = productDto.IsActive
        };

        await _productRepository.AddAsync(product);
        productDto.Id = product.Id; // ID otomatik atanır
        return productDto;
    }

    public async Task UpdateProductAsync(ProductDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(productDto.Id);
        if (product != null)
        {
            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.ImageUrl = productDto.ImageUrl;
            product.CategoryId = productDto.CategoryId;
            product.IsActive = productDto.IsActive;

            await _productRepository.UpdateAsync(product);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }
}
