using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdminBareSkin.Migrations
{
    /// <inheritdoc />
    public partial class CrearModuloFinancieroCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CierresMensuales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Automatico = table.Column<bool>(type: "boolean", nullable: false),
                    TotalVentas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CostoProductosVendidos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GananciaBruta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GastosOperativos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GananciaNetaOperativa = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ComprasInventario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IngresosRegistrados = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EgresosRegistrados = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnidadesVendidas = table.Column<int>(type: "integer", nullable: false),
                    CantidadVentas = table.Column<int>(type: "integer", nullable: false),
                    FechaDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CerradoPor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CierreAnteriorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresMensuales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CierresMensuales_CierresMensuales_CierreAnteriorId",
                        column: x => x.CierreAnteriorId,
                        principalTable: "CierresMensuales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CuentasFinancieras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasFinancieras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistorialCierres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CierreMensualId = table.Column<int>(type: "integer", nullable: false),
                    Accion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Usuario = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCierres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialCierres_CierresMensuales_CierreMensualId",
                        column: x => x.CierreMensualId,
                        principalTable: "CierresMensuales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosFinancieros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CuentaFinancieraId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Concepto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegistradoPor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AutorizadoPor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EsGastoOperativo = table.Column<bool>(type: "boolean", nullable: false),
                    EsCompraInventario = table.Column<bool>(type: "boolean", nullable: false),
                    VentaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosFinancieros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosFinancieros_CuentasFinancieras_CuentaFinancieraId",
                        column: x => x.CuentaFinancieraId,
                        principalTable: "CuentasFinancieras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosFinancieros_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CierresMensuales_CierreAnteriorId",
                table: "CierresMensuales",
                column: "CierreAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCierres_CierreMensualId",
                table: "HistorialCierres",
                column: "CierreMensualId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinancieros_CuentaFinancieraId",
                table: "MovimientosFinancieros",
                column: "CuentaFinancieraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosFinancieros_VentaId",
                table: "MovimientosFinancieros",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialCierres");

            migrationBuilder.DropTable(
                name: "MovimientosFinancieros");

            migrationBuilder.DropTable(
                name: "CierresMensuales");

            migrationBuilder.DropTable(
                name: "CuentasFinancieras");
        }
    }
}
