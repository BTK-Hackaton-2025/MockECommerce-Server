using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DAL.Enums;
using MockECommerce.DtoLayer.ProductDtos;

namespace MockECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }


    /// Tüm ürünleri getirir (Admin only)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(new { success = true, data = products });
    }


    /// Aktif ürünleri getirir (Public)
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveProducts()
    {
        var products = await _productService.GetActiveProductsAsync();
        return Ok(new { success = true, data = products });
    }

    /// ID'ye göre ürün getirir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz ürün ID'si" });

        var product = await _productService.GetProductByIdAsync(id);
        return Ok(new { success = true, data = product });
    }


    /// Kategoriye göre ürünleri getirir
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kategori ID'si" });

        var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
        return Ok(new { success = true, data = products });
    }


    /// Satıcıya göre ürünleri getirir (Seller için kendi ürünleri, Admin için herhangi bir satıcının ürünleri)
    [HttpGet("seller/{sellerId}")]
    [Authorize]
    public async Task<IActionResult> GetProductsBySeller(Guid sellerId)
    {
        if (sellerId == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz satıcı ID'si" });

        // Sadece satıcıların kendi ürünlerini görmesine izin verilir, adminler hariç
        var currentUserId = GetCurrentUserId();
        if (!User.IsInRole("Admin") && sellerId != currentUserId)
            return Forbid();

        var products = await _productService.GetProductsBySellerIdAsync(sellerId);
        return Ok(new { success = true, data = products });
    }


    /// Mevcut kullanıcının ürünlerini getirir (Seller only)
    [HttpGet("my-products")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyProducts()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kullanıcı bilgisi" });
            
        var products = await _productService.GetProductsByUserIdAsync(userId);
        return Ok(new { success = true, data = products });
    }


    /// Durum bazında ürünleri getirir (Admin only)
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProductsByStatus(ProductStatus status)
    {
        var products = await _productService.GetProductsByStatusAsync(status);
        return Ok(new { success = true, data = products });
    }

    /// Ürün arama
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest(new { success = false, message = "Arama terimi gereklidir" });

        var products = await _productService.SearchProductsAsync(searchTerm);
        return Ok(new { success = true, data = products });
    }


    /// Fiyat aralığına göre ürün arama
    [HttpGet("price-range")]
    public async Task<IActionResult> GetProductsByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        var products = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice);
        return Ok(new { success = true, data = products });
    }


    /// Seller profili oluşturur (Seller only)
    [HttpPost("create-seller-profile")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateSellerProfile([FromBody] CreateSellerProfileRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kullanıcı bilgisi" });

        // Check if seller profile already exists
        var existingProfile = await _productService.GetSellerProfileByUserIdAsync(currentUserId);
        if (existingProfile != null)
            return BadRequest(new { success = false, message = "Satıcı profili zaten mevcut" });

        var sellerProfile = await _productService.CreateSellerProfileAsync(currentUserId, request.StoreName);
        return Ok(new { success = true, data = sellerProfile });
    }

    /// Yeni ürün oluşturur (Admin ve Seller)
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Geçersiz veri", errors = ModelState });

        var sellerId = GetCurrentUserId();
        var product = await _productService.CreateProductAsync(createProductDto, sellerId);
        
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, 
            new { success = true, data = product });
    }


    /// Ürün günceller (Seller - kendi ürünü, Admin - herhangi bir ürün)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz ürün ID'si" });

        if (id != updateProductDto.Id)
            return BadRequest(new { success = false, message = "ID uyuşmuyor" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Geçersiz veri", errors = ModelState });

        // Admin kullanıcıları için özel güncelleme mantığı
        if (User.IsInRole("Admin"))
        {
            var product = await _productService.UpdateProductAsAdminAsync(updateProductDto);
            return Ok(new { success = true, data = product });
        }
        else
        {
            // Seller için normal güncelleme
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return BadRequest(new { success = false, message = "Geçersiz kullanıcı bilgisi" });
                
            var product = await _productService.UpdateProductAsync(updateProductDto, currentUserId);
            return Ok(new { success = true, data = product });
        }
    }

    /// Ürün siler (Seller - kendi ürünü, Admin - herhangi bir ürün)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz ürün ID'si" });

        // Admin kullanıcıları için özel silme mantığı
        if (User.IsInRole("Admin"))
        {
            await _productService.DeleteProductAsAdminAsync(id);
        }
        else
        {
            // Seller için normal silme
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
                return BadRequest(new { success = false, message = "Geçersiz kullanıcı bilgisi" });
                
            await _productService.DeleteProductAsync(id, currentUserId);
        }
        
        return Ok(new { success = true, message = "Ürün başarıyla silindi" });
    }


    /// Ürün durumunu günceller (Admin only)
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProductStatus(Guid id, [FromBody] ProductStatus status)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz ürün ID'si" });

        var product = await _productService.UpdateProductStatusAsync(id, status);
        return Ok(new { success = true, data = product });
    }

    /// <summary>
    /// Ürün aktiflik durumunu değiştirir (Admin only)
    /// </summary>
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleProductActiveStatus(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz ürün ID'si" });

        var product = await _productService.ToggleProductActiveStatusAsync(id);
        return Ok(new { success = true, data = product });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("uuid") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}
