using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoPICGestiónRecepción.Data
{
    [Table("RegistroEstiba")]
    public class RegistroEstiba
    {
        [Key]
        public int IdRegistro { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFinal { get; set; }

        public byte[] Firma { get; set; }

        [MaxLength(300)]
        public string Observaciones { get; set; }

        // Relaciones
        [ForeignKey("Cliente")]
        public int IdCliente { get; set; }
        public Cliente Cliente { get; set; }

        [ForeignKey("Vehiculo")]
        public int IdVehiculo { get; set; }
        public Vehiculo Vehiculo { get; set; }

        [ForeignKey("Proveedor")]
        public int IdProveedor { get; set; }
        public Proveedor Proveedor { get; set; }

        [ForeignKey("ResponsableEstiba")]
        public int IdResponsable { get; set; }
        public ResponsableEstiba ResponsableEstiba { get; set; }
    }
}

