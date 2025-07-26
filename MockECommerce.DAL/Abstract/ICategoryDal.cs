using MockECommerce.DAL.Entities;

namespace MockECommerce.DAL.Abstract;

public interface ICategoryDal : IGenericDal<Category>
{
    Task<List<Category>> GetAllWithProductsAsync();

    Task<Category?> GetByIdWitchProductsAsync(Guid id);
}