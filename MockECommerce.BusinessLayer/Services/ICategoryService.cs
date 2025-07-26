using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.CategoryDtos;

namespace MockECommerce.BusinessLayer.Services;

public interface ICategoryService : IGenericService<Category>
{
    // Entity-based methods
    Task<List<Category>> TGetAllWithProductsAsync();
    Task<Category?> TGetByIdWithProductsAsync(Guid id);

    // DTO-based methods for API layer
    Task<CategoryDetailDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<CategoryDetailDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto);
    Task DeleteCategoryAsync(Guid id);
    Task<CategoryDetailDto?> GetCategoryByIdAsync(Guid id);
    Task<List<CategoryListDto>> GetAllCategoriesAsync();
    Task<List<CategoryListDto>> GetHierarchicalCategoriesAsync();
    Task<CategoryDetailDto?> GetCategoryWithChildrenAsync(Guid id);
    Task<List<CategoryListDto>> GetRootCategoriesAsync();
    Task<List<CategoryListDto>> GetChildCategoriesAsync(Guid parentId);
}
