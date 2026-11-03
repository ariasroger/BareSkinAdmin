
using System.ComponentModel.DataAnnotations;

namespace AdminBareSkin.Models
{
    public class CuentaFinanciera
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activa { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<MovimientoFinanciero> Movimientos { get; set; }
            = new List<MovimientoFinanciero>();
    }
}
