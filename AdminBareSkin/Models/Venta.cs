
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminBareSkin.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [StringLength(120)]
        public string? Cliente { get; set; }

        [Required]
        [StringLength(30)]
        public string MetodoPago { get; set; } = "Efectivo";

        [Column(TypeName = "numeric(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Ganancia { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string? Usuario { get; set; }

        public ICollection<DetalleVenta> Detalles { get; set; }
            = new List<DetalleVenta>();
    }
}
