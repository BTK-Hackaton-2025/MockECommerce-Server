using Microsoft.AspNetCore.Mvc;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DtoLayer.ProductDtos;
using System.Security.Claims;

namespace MockECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/v1/external/products")]
public class ExternalProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ExternalProductController(IProductService productService)
    {
        _productService = productService;
    }

    private Guid GetSellerIdFromClaims()
    {
        var sellerIdClaim = User.FindFirst("SellerId")?.Value;
        if (string.IsNullOrEmpty(sellerIdClaim) || !Guid.TryParse(sellerIdClaim, out var sellerId))
            throw new InvalidOperationException("Seller ID is missing or invalid in claims.");

        return sellerId;
    }

    
    /// Get all products for the authenticated seller
    
    [HttpGet]
    public async Task<IActionResult> GetSellerProductsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var sellerId = GetSellerIdFromClaims();
        
        // Use existing method - pagination will be handled in the future
        var allProducts = await _productService.GetProductsBySellerIdAsync(sellerId);
        
        // Simple pagination logic
        var pagedProducts = allProducts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            
        return Ok(new 
        {
            products = pagedProducts,
            totalCount = allProducts.Count,
            page = page,
            pageSize = pageSize,
            totalPages = (int)Math.Ceiling((double)allProducts.Count / pageSize)
        });
    }

    
    /// Get a specific product by ID (only if it belongs to the seller)
    
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProductByIdAsync(Guid productId)
    {
        var sellerId = GetSellerIdFromClaims();
        
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("Product not found");

        // Verify the product belongs to the authenticated seller
        if (product.SellerId != sellerId)
            return Forbid("You don't have access to this product");

        return Ok(product);
    }

    
    /// Create a new product
   
    [HttpPost]
    public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductDto createProductDto)
    {
        var sellerId = GetSellerIdFromClaims();
        
        // Set the seller ID from the authenticated API key
        var product = await _productService.CreateProductAsync(createProductDto, sellerId);
        
        return CreatedAtAction(nameof(GetProductByIdAsync), new { productId = product.Id }, product);
    }

    
    /// Update an existing product
    
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProductAsync(Guid productId, [FromBody] UpdateProductDto updateProductDto)
    {
        var sellerId = GetSellerIdFromClaims();

        // Verify the product exists and belongs to the seller
        var existingProduct = await _productService.GetProductByIdAsync(productId);
        if (existingProduct == null)
            return NotFound("Product not found");

        if (existingProduct.SellerId != sellerId)
            return Forbid("You don't have access to this product");

        // Ensure the ID matches
        updateProductDto.Id = productId;

        var updatedProduct = await _productService.UpdateProductAsync(updateProductDto, sellerId);
        return Ok(updatedProduct);
    }

    
    /// Delete a product
    
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProductAsync(Guid productId)
    {
        var sellerId = GetSellerIdFromClaims();

        // Verify the product exists and belongs to the seller
        var existingProduct = await _productService.GetProductByIdAsync(productId);
        if (existingProduct == null)
            return NotFound("Product not found");

        if (existingProduct.SellerId != sellerId)
            return Forbid("You don't have access to this product");

        await _productService.DeleteProductAsync(productId, sellerId);
        return NoContent();
    }

    
    /// Get basic statistics for the seller's products
    
    [HttpGet("statistics")]
    public async Task<IActionResult> GetProductStatisticsAsync()
    {
        var sellerId = GetSellerIdFromClaims();
        
        // Get all products for the seller and calculate basic statistics
        var products = await _productService.GetProductsBySellerIdAsync(sellerId);
        
        var stats = new 
        {
            totalProducts = products.Count,
            activeProducts = products.Count(p => p.IsActive),
            inactiveProducts = products.Count(p => !p.IsActive),
            draftProducts = products.Count(p => p.Status == MockECommerce.DAL.Enums.ProductStatus.Draft),
            activeStatusProducts = products.Count(p => p.Status == MockECommerce.DAL.Enums.ProductStatus.Active),
            blockedProducts = products.Count(p => p.Status == MockECommerce.DAL.Enums.ProductStatus.Blocked),
            totalValue = products.Where(p => p.IsActive).Sum(p => p.Price * p.Stock)
        };
        
        return Ok(stats);
    }
}
