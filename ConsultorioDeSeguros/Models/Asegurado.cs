namespace ConsultorioDeSeguros.Models
{
    public class Asegurado
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; } 
        public string Telefono { get; set; }
        public int Edad { get; set; }
        public ICollection<Seguro> Seguros { get; set; } = new List<Seguro>();
    }

}

