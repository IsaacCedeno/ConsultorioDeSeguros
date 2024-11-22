using ConsultorioDeSeguros.Models;
using ConsultorioDeSeguros.Persistences.Interfaces;
using System.Data.SqlClient;

namespace ConsultorioDeSeguros.Persistences.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly string _connectionString;

        public GenericRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RegisterAsync(T entity)
        {
            if (entity is Asegurado asegurado)
            {
                string query = "INSERT INTO Asegurado (Cedula, NombreCliente, Telefono, Edad) " +
                               "OUTPUT INSERTED.Id VALUES (@Cedula, @NombreCliente, @Telefono, @Edad)";

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Cedula", asegurado.Cedula);
                        cmd.Parameters.AddWithValue("@NombreCliente", asegurado.Nombre);
                        cmd.Parameters.AddWithValue("@Telefono", (object)asegurado.Telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Edad", asegurado.Edad);


                        asegurado.Id = (int)await cmd.ExecuteScalarAsync();
                    }
                }
            }
            else if (entity is Seguro seguro)
            {
                string query = "INSERT INTO Seguro (NombreSeguro, SumaAsegurada, Prima) VALUES (@NombreSeguro, @SumaAsegurada, @Prima)";

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@NombreSeguro", seguro.Nombre);
                        cmd.Parameters.AddWithValue("@SumaAsegurada", seguro.SumaAsegurada);
                        cmd.Parameters.AddWithValue("@Prima", seguro.Prima);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task EditAsync(T entity)
        {
            if (entity is Asegurado asegurado)
            {
                string query = "UPDATE Asegurado SET Cedula = @Cedula, NombreCliente = @NombreCliente, Telefono = @Telefono, Edad = @Edad WHERE Id = @Id";

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", asegurado.Id);
                        cmd.Parameters.AddWithValue("@Cedula", asegurado.Cedula);
                        cmd.Parameters.AddWithValue("@NombreCliente", asegurado.Nombre);
                        cmd.Parameters.AddWithValue("@Telefono", asegurado.Telefono);
                        cmd.Parameters.AddWithValue("@Edad", asegurado.Edad);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            else if (entity is Seguro seguro)
            {
                string query = "UPDATE Seguro SET NombreSeguro = @NombreSeguro, SumaAsegurada = @SumaAsegurada, Prima = @Prima WHERE CodigoSeguro = @Id";

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", seguro.Id);
                        cmd.Parameters.AddWithValue("@NombreSeguro", seguro.Nombre);
                        cmd.Parameters.AddWithValue("@SumaAsegurada", seguro.SumaAsegurada);
                        cmd.Parameters.AddWithValue("@Prima", seguro.Prima);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            string query = typeof(T) == typeof(Asegurado) ?
                "DELETE FROM AseguradoSeguro where AseguradoId = @Id; DELETE FROM Asegurado where Id = @Id" :
                "DELETE FROM Seguro WHERE CodigoSeguro = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var list = new List<T>();
            string query = typeof(T) == typeof(Asegurado) ?
     @"SELECT a.Id, a.Cedula, a.NombreCliente, a.Telefono, a.Edad,
             s.CodigoSeguro, s.NombreSeguro, s.SumaAsegurada, s.Prima
      FROM Asegurado a
      LEFT JOIN AseguradoSeguro asu ON asu.AseguradoId = a.Id
      LEFT JOIN Seguro s ON s.CodigoSeguro = asu.CodigoSeguro" :
     "SELECT CodigoSeguro, NombreSeguro, SumaAsegurada, Prima FROM Seguro";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var aseguradosDict = new Dictionary<int, Asegurado>();

                        while (await reader.ReadAsync())
                        {
                            if (typeof(T) == typeof(Asegurado))
                            {
                                int id = reader.GetInt32(reader.GetOrdinal("Id"));

                                if (!aseguradosDict.TryGetValue(id, out var asegurado))
                                {
                                    asegurado = new Asegurado
                                    {
                                        Id = id,
                                        Cedula = reader.GetString(reader.GetOrdinal("Cedula")),
                                        Nombre = reader.GetString(reader.GetOrdinal("NombreCliente")),
                                        Telefono = reader.GetString(reader.GetOrdinal("Telefono")),
                                        Edad = reader.GetInt32(reader.GetOrdinal("Edad")),
                                        Seguros = new List<Seguro>()
                                    };

                                    aseguradosDict.Add(id, asegurado);
                                }


                                if (!reader.IsDBNull(reader.GetOrdinal("CodigoSeguro")))
                                {
                                    var seguro = new Seguro
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("CodigoSeguro")),
                                        Nombre = reader.GetString(reader.GetOrdinal("NombreSeguro")),
                                        SumaAsegurada = reader.GetDecimal(reader.GetOrdinal("SumaAsegurada")),
                                        Prima = reader.GetDecimal(reader.GetOrdinal("Prima"))
                                    };
                                    asegurado.Seguros.Add(seguro);
                                }
                            }
                            else if (typeof(T) == typeof(Seguro))
                            {
                                var seguro = new Seguro
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CodigoSeguro")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NombreSeguro")),
                                    SumaAsegurada = reader.GetDecimal(reader.GetOrdinal("SumaAsegurada")),
                                    Prima = reader.GetDecimal(reader.GetOrdinal("Prima"))
                                };
                                list.Add(seguro as T);
                            }
                        }


                        if (typeof(T) == typeof(Asegurado))
                        {
                            list.AddRange(aseguradosDict.Values as IEnumerable<T>);
                        }
                    }
                }
            }

            return list;
        }

        public async Task<T> GetById(int id)
        {
            string query = typeof(T) == typeof(Asegurado) ?
                "SELECT Id, Cedula, NombreCliente, Telefono, Edad FROM Asegurado WHERE Id = @Id" :
                "SELECT CodigoSeguro, NombreSeguro, SumaAsegurada, Prima FROM Seguro WHERE CodigoSeguro = @Id";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (typeof(T) == typeof(Asegurado))
                            {
                                return (T)(object)new Asegurado
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Cedula = reader.GetString(reader.GetOrdinal("Cedula")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NombreCliente")),
                                    Telefono = reader.GetString(reader.GetOrdinal("Telefono")),
                                    Edad = reader.GetInt32(reader.GetOrdinal("Edad"))
                                };
                            }
                            else if (typeof(T) == typeof(Seguro))
                            {
                                return (T)(object)new Seguro
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CodigoSeguro")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NombreSeguro")),
                                    SumaAsegurada = reader.GetDecimal(reader.GetOrdinal("SumaAsegurada")),
                                    Prima = reader.GetDecimal(reader.GetOrdinal("Prima"))
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
