using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdminBareSkin.Migrations
{
    public partial class CrearModuloVentas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    Cliente = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: true),

                    MetodoPago = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false),

                    Total = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false),

                    Ganancia = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false),

                    FechaRegistro = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false),

                    Usuario = table.Column<string>(
                        type: "character varying(256)",
                        maxLength: 256,
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),

                    VentaId = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    ProductoId = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    Cantidad = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    PrecioUnitario = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false),

                    PrecioCompraUnitario = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false),

                    Subtotal = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false),

                    Ganancia = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVenta", x => x.Id);

                    table.ForeignKey(
                        name: "FK_DetallesVenta_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_DetallesVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetallesVenta",
                column: "VentaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DetallesVenta");
            migrationBuilder.DropTable(name: "Ventas");
        }
    }
}