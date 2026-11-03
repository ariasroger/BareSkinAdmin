
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminBareSkin.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        public int VentaId { get; set; }

        public Venta Venta { get; set; } = null!;

        public int ProductoId { get; set; }

        public Producto Producto { get; set; } = null!;

        [Range(1, 999999)]
        public int Cantidad { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal PrecioCompraUnitario { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal Ganancia { get; set; }
    }
}
