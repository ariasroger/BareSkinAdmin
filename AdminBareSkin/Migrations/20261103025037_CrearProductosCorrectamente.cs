
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminBareSkin.Migrations
{
    public partial class CrearProductosCorrectamente : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabla Productos ya existe en Supabase.
            // No es necesario volver a crearla.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Se conserva la tabla Productos y sus datos.
        }
    }
}
