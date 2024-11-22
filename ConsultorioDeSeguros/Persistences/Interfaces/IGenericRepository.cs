namespace ConsultorioDeSeguros.Persistences.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetById(int id);
        Task RegisterAsync(T entity);
        Task EditAsync(T entity);
        Task DeleteAsync(int id);

    }
}
