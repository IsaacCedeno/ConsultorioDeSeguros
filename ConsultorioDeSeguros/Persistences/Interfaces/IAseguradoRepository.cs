using ConsultorioDeSeguros.Models;

namespace ConsultorioDeSeguros.Persistences.Interfaces
{
    public interface IAseguradoRepository : IGenericRepository<Asegurado> 
    {
        Task<IEnumerable<Asegurado>> GetByCiAsync(string cedula);
        Task CargarData(IFormFile file);
        Task AsociarSeguroAsync(int aseguradoId, int codigoSeguro);
        Task<IEnumerable<Seguro>> GetSegurosByAseguradoId(int aseguradoId);
        Task AsignarSegurosPorEdad(Asegurado asegurado);
    }

    //Realizamos un cambio en el sistema
}
