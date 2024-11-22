using ConsultorioDeSeguros.Models;

namespace ConsultorioDeSeguros.Persistences.Repositories
{
    public class SeguroRepository : GenericRepository<Seguro>
    {
        private readonly string _connectionString;

        public SeguroRepository(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        
    }
}
