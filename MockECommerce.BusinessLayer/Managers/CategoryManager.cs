using AutoMapper;
using MockECommerce.BusinessLayer.Exceptions;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DAL.Abstract;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.CategoryDtos;

namespace MockECommerce.BusinessLayer.Managers;

public class CategoryManager : ICategoryService
{
    private readonly ICategoryDal _categoryDal;
    private readonly IMapper _mapper;

    public CategoryManager(ICategoryDal categoryDal, IMapper mapper)
    {
        _categoryDal = categoryDal;
        _mapper = mapper;
    }

    public async Task TAddAsync(Category entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(entity.CategoryName))
            throw new BusinessException("Kategori adı boş olamaz.", "CATEGORY_NAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(entity.SeoSlug))
            throw new BusinessException("SEO slug boş olamaz.", "SEO_SLUG_REQUIRED");

        // Parent kategori varsa kontrol et
        if (entity.ParentId.HasValue)
        {
            var parentCategory = await _categoryDal.GetByIdAsync(entity.ParentId.Value);
            if (parentCategory == null)
                throw new NotFoundException($"Parent kategori bulunamadı. ID: {entity.ParentId}", "PARENT_CATEGORY_NOT_FOUND");
        }

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.IsActive = true;

        await _categoryDal.CreateAsync(entity);
    }

    public async Task TDeleteAsync(Category entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existingCategory = await _categoryDal.GetByIdAsync(entity.Id);
        if (existingCategory == null)
            throw new NotFoundException($"Kategori bulunamadı. ID: {entity.Id}", "CATEGORY_NOT_FOUND");

        // Alt kategorileri kontrol et
        var allCategories = await _categoryDal.GetAllAsync();
        var hasChildren = allCategories.Any(c => c.ParentId == entity.Id);
        if (hasChildren)
            throw new BusinessException("Alt kategorileri olan kategori silinemez.", "CATEGORY_HAS_CHILDREN");

        await _categoryDal.DeleteAsync(existingCategory);
    }

    public async Task TUpdateAsync(Category entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(entity.CategoryName))
            throw new BusinessException("Kategori adı boş olamaz.", "CATEGORY_NAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(entity.SeoSlug))
            throw new BusinessException("SEO slug boş olamaz.", "SEO_SLUG_REQUIRED");

        var existingCategory = await _categoryDal.GetByIdAsync(entity.Id);
        if (existingCategory == null)
            throw new NotFoundException($"Kategori bulunamadı. ID: {entity.Id}", "CATEGORY_NOT_FOUND");

        // Parent kategori varsa kontrol et
        if (entity.ParentId.HasValue)
        {
            // Kendisini parent olarak seçemez
            if (entity.ParentId == entity.Id)
                throw new BusinessException("Kategori kendisinin parent'ı olamaz.", "INVALID_PARENT_CATEGORY");

            var parentCategory = await _categoryDal.GetByIdAsync(entity.ParentId.Value);
            if (parentCategory == null)
                throw new NotFoundException($"Parent kategori bulunamadı. ID: {entity.ParentId}", "PARENT_CATEGORY_NOT_FOUND");

            // Circular reference kontrolü
            if (await IsCircularReference(entity.Id, entity.ParentId.Value))
                throw new BusinessException("Circular reference oluşturacak parent kategori seçilemez.", "CIRCULAR_REFERENCE");
        }

        // Güncelleme
        existingCategory.CategoryName = entity.CategoryName;
        existingCategory.SeoSlug = entity.SeoSlug;
        existingCategory.ParentId = entity.ParentId;
        existingCategory.IsActive = entity.IsActive;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        await _categoryDal.UpdateAsync(existingCategory);
    }

    public async Task<Category?> TGetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID boş olamaz.", nameof(id));

        return await _categoryDal.GetByIdAsync(id);
    }

    public async Task<List<Category>> TGetAllAsync()
    {
        return await _categoryDal.GetAllAsync();
    }

    public async Task<List<Category>> TGetAllWithProductsAsync()
    {
        return await _categoryDal.GetAllWithProductsAsync();
    }

    public async Task<Category?> TGetByIdWithProductsAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID boş olamaz.", nameof(id));

        return await _categoryDal.GetByIdWitchProductsAsync(id);
    }

    // DTO-based methods for API layer
    public async Task<CategoryDetailDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        if (createCategoryDto == null)
            throw new ArgumentNullException(nameof(createCategoryDto));

        var category = _mapper.Map<Category>(createCategoryDto);
        await TAddAsync(category);

        return _mapper.Map<CategoryDetailDto>(category);
    }

    public async Task<CategoryDetailDto> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
    {
        if (updateCategoryDto == null)
            throw new ArgumentNullException(nameof(updateCategoryDto));

        var category = _mapper.Map<Category>(updateCategoryDto);
        await TUpdateAsync(category);

        var updatedCategory = await _categoryDal.GetByIdAsync(category.Id);
        return _mapper.Map<CategoryDetailDto>(updatedCategory!);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID boş olamaz.", nameof(id));

        var category = await _categoryDal.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException($"Kategori bulunamadı. ID: {id}", "CATEGORY_NOT_FOUND");

        await TDeleteAsync(category);
    }

    public async Task<CategoryDetailDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await TGetByIdAsync(id);
        if (category == null)
            return null;

        var categoryDto = _mapper.Map<CategoryDetailDto>(category);
        
        // Parent name'i set et
        if (category.ParentId.HasValue)
        {
            var parent = await _categoryDal.GetByIdAsync(category.ParentId.Value);
            categoryDto.ParentName = parent?.CategoryName;
        }

        return categoryDto;
    }

    public async Task<List<CategoryListDto>> GetAllCategoriesAsync()
    {
        var categories = await TGetAllAsync();
        return _mapper.Map<List<CategoryListDto>>(categories);
    }

    public async Task<List<CategoryListDto>> GetHierarchicalCategoriesAsync()
    {
        var allCategories = await TGetAllAsync();
        var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
        
        var result = new List<CategoryListDto>();
        
        foreach (var rootCategory in rootCategories)
        {
            var categoryDto = _mapper.Map<CategoryListDto>(rootCategory);
            categoryDto.Children = await GetChildrenRecursive(rootCategory.Id, allCategories);
            result.Add(categoryDto);
        }
        
        return result;
    }

    public async Task<CategoryDetailDto?> GetCategoryWithChildrenAsync(Guid id)
    {
        var category = await TGetByIdAsync(id);
        if (category == null)
            return null;

        var categoryDto = _mapper.Map<CategoryDetailDto>(category);
        
        // Parent name'i set et
        if (category.ParentId.HasValue)
        {
            var parent = await _categoryDal.GetByIdAsync(category.ParentId.Value);
            categoryDto.ParentName = parent?.CategoryName;
        }

        // Children'ı set et
        var allCategories = await TGetAllAsync();
        categoryDto.Children = await GetChildrenRecursive(id, allCategories);
        
        return categoryDto;
    }

    public async Task<List<CategoryListDto>> GetRootCategoriesAsync()
    {
        var allCategories = await TGetAllAsync();
        var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
        return _mapper.Map<List<CategoryListDto>>(rootCategories);
    }

    public async Task<List<CategoryListDto>> GetChildCategoriesAsync(Guid parentId)
    {
        var allCategories = await TGetAllAsync();
        var childCategories = allCategories.Where(c => c.ParentId == parentId).ToList();
        return _mapper.Map<List<CategoryListDto>>(childCategories);
    }

    // Yardımcı metodlar
    private async Task<List<CategoryListDto>> GetChildrenRecursive(Guid parentId, List<Category> allCategories)
    {
        var children = allCategories.Where(c => c.ParentId == parentId).ToList();
        var result = new List<CategoryListDto>();
        
        foreach (var child in children)
        {
            var childDto = _mapper.Map<CategoryListDto>(child);
            childDto.Children = await GetChildrenRecursive(child.Id, allCategories);
            result.Add(childDto);
        }
        
        return result;
    }

    private async Task<bool> IsCircularReference(Guid categoryId, Guid parentId)
    {
        var allCategories = await _categoryDal.GetAllAsync();
        var visited = new HashSet<Guid>();
        var current = parentId;

        while (current != Guid.Empty && !visited.Contains(current))
        {
            if (current == categoryId)
                return true;

            visited.Add(current);
            var category = allCategories.FirstOrDefault(c => c.Id == current);
            current = category?.ParentId ?? Guid.Empty;
        }

        return false;
    }
}
