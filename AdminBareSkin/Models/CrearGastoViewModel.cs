
using System.ComponentModel.DataAnnotations;

namespace AdminBareSkin.Models
{
    public class CrearGastoViewModel
    {
        [Required(ErrorMessage = "Ingresá el concepto del gasto.")]
        [StringLength(150)]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá una descripción.")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná una categoría.")]
        public string Categoria { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Monto { get; set; }

        [Required]
        public int CuentaFinancieraId { get; set; }

        [Required]
        public string TipoGasto { get; set; } = "Operativo";
    }
}
