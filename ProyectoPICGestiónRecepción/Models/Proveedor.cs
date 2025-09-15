using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPICGestiónRecepción.Data
{
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [ForeignKey("Representante")]
        public int IdRepresentante { get; set; }

        public virtual Representante Representante { get; set; }  // virtual para Lazy Loading
    }
}


