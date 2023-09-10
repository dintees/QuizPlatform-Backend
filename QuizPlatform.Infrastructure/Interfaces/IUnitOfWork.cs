namespace QuizPlatform.Infrastructure.Interfaces
{
    public interface IUnitOfWork<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Func<T, bool> predicate);
        T? GetOne(Func<T, bool> predicate);
        Task AddAsync(T entity);
        Task<bool> SaveAsync();
    }
}
