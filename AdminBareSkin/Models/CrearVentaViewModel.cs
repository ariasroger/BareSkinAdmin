
using System.ComponentModel.DataAnnotations;

namespace AdminBareSkin.Models
{
    public class CrearVentaViewModel
    {
        [StringLength(120, ErrorMessage = "El nombre del cliente es demasiado largo.")]
        public string? Cliente { get; set; }

        [Required(ErrorMessage = "Seleccioná un método de pago.")]
        public string MetodoPago { get; set; } = "Efectivo";

        public List<DetalleVentaInput> Detalles { get; set; } = new();
    }

    public class DetalleVentaInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un producto.")]
        public int ProductoId { get; set; }

        [Range(1, 999999, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; } = 1;
    }
}
