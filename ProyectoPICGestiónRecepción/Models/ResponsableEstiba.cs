using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoPICGestiónRecepción.Data
{
    [Table("ResponsableEstiba")]
    public class ResponsableEstiba
    {
        [Key]
        public int IdResponsable { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(100)]
        public string Apellido { get; set; }

        [Required, MaxLength(10)]
        public string Cedula { get; set; }
    }


}

