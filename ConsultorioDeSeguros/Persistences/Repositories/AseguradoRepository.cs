using ConsultorioDeSeguros.Models;
using ConsultorioDeSeguros.Persistences.Interfaces;
using System.Data.SqlClient;

namespace ConsultorioDeSeguros.Persistences.Repositories
{
    public class AseguradoRepository : GenericRepository<Asegurado>, IAseguradoRepository
    {
        private readonly string _connectionString;

        public AseguradoRepository(string connectionString) : base(connectionString) 
        {
            _connectionString = connectionString;    
        }

        public async Task<IEnumerable<Asegurado>> GetByCiAsync(string cedula)
        {
            var asegurados = new List<Asegurado>();
            var query = "SELECT * FROM Asegurado WHERE Cedula = @Cedula";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Cedula", cedula);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var asegurado = new Asegurado
                            {
                                Id = (int)reader["Id"],
                                Cedula = reader["Cedula"].ToString(),
                                Nombre = reader["NombreCliente"].ToString(),
                                Telefono = reader["Telefono"].ToString()!,
                                Edad = (int)reader["Edad"]
                            };

                            asegurados.Add(asegurado);
                        }
                    }
                }
            }

            return asegurados;
        }

        public async Task AsociarSeguroAsync(int aseguradoId, int codigoSeguro)
        {
            var query = "INSERT INTO AseguradoSeguro (AseguradoId, CodigoSeguro) " +
                "VALUES (@AseguradoId, @CodigoSeguro)";

            using (var connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AseguradoId", aseguradoId);
                    cmd.Parameters.AddWithValue("@CodigoSeguro", codigoSeguro);

                    await cmd.ExecuteScalarAsync();
                }
            }
        }

        public async Task CargarData(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No se ha cargado ningún archivo.");

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var datos = line.Split(',');

                    var asegurado = new Asegurado
                    {
                        Cedula = datos[0],
                        Nombre = datos[1],
                        Telefono = datos[2],
                        Edad = int.Parse(datos[3])
                    };

                    await RegisterAsync(asegurado);

                    var aseguradoRegistrado = (await GetByCiAsync(asegurado.Cedula)).FirstOrDefault();
                    
                    if (aseguradoRegistrado != null) {
                        await AsignarSegurosPorEdad(asegurado);
                    }
                     
                }
            }
        }

        public async Task<IEnumerable<Seguro>> GetSegurosByAseguradoId(int aseguradoId)
        {
            var seguros = new List<Seguro>();
            var query = "SELECT s.* FROM Seguro s " +
                        "JOIN AseguradoSeguro asg ON s.CodigoSeguro = asg.CodigoSeguro " +
                        "WHERE asg.AseguradoId = @AseguradoId";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@AseguradoId", aseguradoId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var seguro = new Seguro
                            {
                                Id = (int)reader["CodigoSeguro"],
                                Nombre = reader["NombreSeguro"].ToString(),
                                SumaAsegurada = (decimal)reader["SumaAsegurada"],
                                Prima = (decimal)reader["Prima"]
                            };

                            seguros.Add(seguro);
                        }
                    }
                }
            }

            return seguros;
        }


        public async Task AsignarSegurosPorEdad(Asegurado asegurado)
        {
            if (asegurado.Id <= 0) {
                throw new InvalidOperationException("El asegurado no ha sido guardado en la base de datos");
            }

            var seguros = await new GenericRepository<Seguro>(_connectionString).GetAllAsync();

            Seguro seguroAsignar = null;

            try
            {

                if (asegurado.Edad < 20)
                {
                    seguroAsignar = seguros.FirstOrDefault(s => s.Id == 1); 
                }
                else if (asegurado.Edad >= 20 && asegurado.Edad < 30)
                {
                    seguroAsignar = seguros.FirstOrDefault(s => s.Id == 2); 
                }
                else if (asegurado.Edad >= 30 && asegurado.Edad < 40)
                {
                    seguroAsignar = seguros.FirstOrDefault(s => s.Id == 3); 
                }
                else
                {
                    seguroAsignar = seguros.FirstOrDefault(s => s.Id == 4); 
                }

                if (seguroAsignar != null)
                {
                    await AsociarSeguroAsync(asegurado.Id, seguroAsignar.Id);
                }

            }
            catch (Exception ex) {
                throw new Exception("Error al asignar seguro: " + ex);
            }

           
        }





    }
}
