using UnicornHack.Services;

namespace UnicornHack.Data
{
    public class Repository : IRepository
    {
        private readonly GameDbContext _context;

        public Repository(GameDbContext context) => _context = context;

        public T Find<T>(params object[] keyValues) where T : class
            => _context.Find<T>(keyValues);

        public void Add<T>(T entity) where T : class
            => _context.Set<T>().Add(entity);

        public void Delete<T>(T entity) where T : class
        {
            var local = _context.Set<T>().Local;
            if (local.Contains(entity))
            {
                local.Remove(entity);
            }
        }
    }
}
