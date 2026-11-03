
using System.ComponentModel.DataAnnotations;

namespace AdminBareSkin.Models
{
    public class HistorialCierre
    {
        public int Id { get; set; }

        public int CierreMensualId { get; set; }
        public CierreMensual CierreMensual { get; set; } = null!;

        [Required, StringLength(30)]
        public string Accion { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Motivo { get; set; }

        [StringLength(256)]
        public string? Usuario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
