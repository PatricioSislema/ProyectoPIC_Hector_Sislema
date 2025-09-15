using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoPICGestiónRecepción.Data
{
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, MaxLength(13)]
        public string RUC { get; set; }

        [MaxLength(200)]
        public string Direccion { get; set; }

        [MaxLength(15)]
        public string Telefono { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }
    }

}


