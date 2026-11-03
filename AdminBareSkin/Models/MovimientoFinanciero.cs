
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminBareSkin.Models
{
    public class MovimientoFinanciero
    {
        public int Id { get; set; }

        public int CuentaFinancieraId { get; set; }
        public CuentaFinanciera CuentaFinanciera { get; set; } = null!;

        [Required, StringLength(20)]
        public string Tipo { get; set; } = "Egreso";

        [Required, StringLength(80)]
        public string Categoria { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Concepto { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string? RegistradoPor { get; set; }

        [StringLength(256)]
        public string? AutorizadoPor { get; set; }

        // Solo los gastos operativos reducen la ganancia operativa.
        public bool EsGastoOperativo { get; set; }

        // Las compras de inventario no son gastos operativos.
        public bool EsCompraInventario { get; set; }

        // Evita duplicar ingresos de una misma venta.
        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }
    }
}
