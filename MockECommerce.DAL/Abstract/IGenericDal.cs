namespace MockECommerce.DAL.Abstract;

public interface IGenericDal <T> where T : class
{
    Task CreateAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> GetAllAsync();
}