namespace MockECommerce.BusinessLayer.Services;

public interface IGenericService<T> where T : class
{
    // Başında T olan metodlar Business katmanında olanlar

    Task TAddAsync(T entity);

    Task TDeleteAsync(T entity);

    Task TUpdateAsync(T entity);

    Task<T?> TGetByIdAsync(Guid id);

    Task<List<T>> TGetAllAsync();
}