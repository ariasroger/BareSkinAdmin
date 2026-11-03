
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminBareSkin.Models
{
    public class CierreMensual
    {
        public int Id { get; set; }

        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Version { get; set; } = 1;

        [Required, StringLength(20)]
        public string Estado { get; set; } = "Cerrado";

        public bool Automatico { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal TotalVentas { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal CostoProductosVendidos { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal GananciaBruta { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal GastosOperativos { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal GananciaNetaOperativa { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal ComprasInventario { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal IngresosRegistrados { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal EgresosRegistrados { get; set; }

        public int UnidadesVendidas { get; set; }
        public int CantidadVentas { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }

        public DateTime FechaCierre { get; set; } = DateTime.UtcNow;

        [StringLength(256)]
        public string? CerradoPor { get; set; }

        public int? CierreAnteriorId { get; set; }

        public CierreMensual? CierreAnterior { get; set; }

        public ICollection<HistorialCierre> Historial { get; set; }
            = new List<HistorialCierre>();
    }
}
