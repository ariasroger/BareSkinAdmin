
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminBareSkin.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio de compra es obligatorio")]
        [Range(typeof(decimal), "0.01", "999999999",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El precio de compra debe ser mayor que cero")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal PrecioCompra { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(typeof(decimal), "0.01", "999999999",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El precio de venta debe ser mayor que cero")]
        [Column(TypeName = "numeric(18,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [Range(0, 999999)]
        public int Stock { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public bool Activo { get; set; } = true;

        [NotMapped]
        public decimal GananciaPorUnidad => PrecioVenta - PrecioCompra;
    }
}
