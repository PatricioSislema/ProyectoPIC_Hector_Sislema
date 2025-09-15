using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoPICGestiónRecepción.Data
{
    [Table("Vehiculo")]
       
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }

        [Required, MaxLength(20)]
        public string Placa { get; set; }

        [Required, MaxLength(50)]
        public string TipoVehiculo { get; set; }
    }
}


