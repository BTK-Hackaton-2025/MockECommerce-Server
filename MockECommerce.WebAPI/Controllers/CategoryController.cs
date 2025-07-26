using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockECommerce.BusinessLayer.Exceptions;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DtoLayer.CategoryDtos;

namespace MockECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Tüm kategorileri getirir
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(new { success = true, data = categories });
    }

    /// <summary>
    /// Hiyerarşik kategori yapısını getirir
    /// </summary>
    [HttpGet("hierarchical")]
    public async Task<IActionResult> GetHierarchicalCategories()
    {
        var categories = await _categoryService.GetHierarchicalCategoriesAsync();
        return Ok(new { success = true, data = categories });
    }

    /// <summary>
    /// Ana kategorileri getirir (parent'ı olmayan)
    /// </summary>
    [HttpGet("root")]
    public async Task<IActionResult> GetRootCategories()
    {
        var categories = await _categoryService.GetRootCategoriesAsync();
        return Ok(new { success = true, data = categories });
    }

    /// <summary>
    /// ID'ye göre kategori getirir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kategori ID'si" });

        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new { success = false, message = "Kategori bulunamadı" });

        return Ok(new { success = true, data = category });
    }

    /// <summary>
    /// Kategoriyi alt kategorileri ile birlikte getirir
    /// </summary>
    [HttpGet("{id}/with-children")]
    public async Task<IActionResult> GetCategoryWithChildren(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kategori ID'si" });

        var category = await _categoryService.GetCategoryWithChildrenAsync(id);
        if (category == null)
            return NotFound(new { success = false, message = "Kategori bulunamadı" });

        return Ok(new { success = true, data = category });
    }

    /// <summary>
    /// Belirtilen kategoriinin alt kategorilerini getirir
    /// </summary>
    [HttpGet("{parentId}/children")]
    public async Task<IActionResult> GetChildCategories(Guid parentId)
    {
        if (parentId == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz parent kategori ID'si" });

        var categories = await _categoryService.GetChildCategoriesAsync(parentId);
        return Ok(new { success = true, data = categories });
    }

    /// <summary>
    /// Yeni kategori oluşturur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Geçersiz veri", errors = ModelState });

        var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, 
            new { success = true, data = category });
    }

    /// <summary>
    /// Kategori günceller
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kategori ID'si" });

        if (id != updateCategoryDto.Id)
            return BadRequest(new { success = false, message = "ID uyuşmuyor" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Geçersiz veri", errors = ModelState });

        var category = await _categoryService.UpdateCategoryAsync(updateCategoryDto);
        return Ok(new { success = true, data = category });
    }

    /// <summary>
    /// Kategori siler
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Geçersiz kategori ID'si" });

        await _categoryService.DeleteCategoryAsync(id);
        return Ok(new { success = true, message = "Kategori başarıyla silindi" });
    }
}
