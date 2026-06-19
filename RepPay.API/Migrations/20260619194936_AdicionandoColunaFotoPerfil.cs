using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepPay.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoColunaFotoPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FotoPerfil",
                table: "usuario",
                newName: "foto_perfil");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "foto_perfil",
                table: "usuario",
                newName: "FotoPerfil");
        }
    }
}
