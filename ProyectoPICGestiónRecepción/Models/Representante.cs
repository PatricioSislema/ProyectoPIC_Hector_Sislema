using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProyectoPICGestiónRecepción.Data
{
    public class Representante
    {
        [Key]
        public int IdRepresentante { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(100)]
        public string Apellido { get; set; }

        [Required, MaxLength(10)]
        public string Cedula { get; set; }

        public byte[] Firma { get; set; }  // Firma digitalizada

        // Relación con proveedores
        public virtual ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
    }
}


