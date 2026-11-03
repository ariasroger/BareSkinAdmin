
namespace AdminBareSkin.Models
{
    public class ReporteFinancieroViewModel
    {
        public int Anio { get; set; }

        public decimal TotalVentas { get; set; }
        public decimal CostoProductos { get; set; }
        public decimal GananciaBruta { get; set; }
        public decimal GastosOperativos { get; set; }
        public decimal GananciaNeta { get; set; }
        public decimal ComprasInventario { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }

        public int CantidadVentas { get; set; }
        public int UnidadesVendidas { get; set; }

        public List<ReporteMesViewModel> Meses { get; set; } = new();
        public List<ReporteCuentaViewModel> Cuentas { get; set; } = new();
    }

    public class ReporteMesViewModel
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; } = "";

        public decimal Ventas { get; set; }
        public decimal Costos { get; set; }
        public decimal GananciaBruta { get; set; }
        public decimal GastosOperativos { get; set; }
        public decimal GananciaNeta { get; set; }

        public int CantidadVentas { get; set; }
    }

    public class ReporteCuentaViewModel
    {
        public string Nombre { get; set; } = "";

        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal Saldo { get; set; }
    }
}
