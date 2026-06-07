using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepPay.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarColunaAtivoNoGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_codigo_recuperacao",
                table: "codigo_recuperacao");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "parcela",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "despesa",
                newName: "status");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:status_despesa", "ATIVA,QUITADA,CANCELADA")
                .OldAnnotation("Npgsql:Enum:status_parcela", "PENDENTE,PAGO,ATRASADO");

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "parcela",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "grupo",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "despesa",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "despesa",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "codigo_recuperacao_pkey",
                table: "codigo_recuperacao",
                column: "id_codigo");

            migrationBuilder.CreateIndex(
                name: "IX_codigo_recuperacao_id_usuario",
                table: "codigo_recuperacao",
                column: "id_usuario");

            migrationBuilder.AddForeignKey(
                name: "fk_codigo_usuario",
                table: "codigo_recuperacao",
                column: "id_usuario",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_codigo_usuario",
                table: "codigo_recuperacao");

            migrationBuilder.DropPrimaryKey(
                name: "codigo_recuperacao_pkey",
                table: "codigo_recuperacao");

            migrationBuilder.DropIndex(
                name: "IX_codigo_recuperacao_id_usuario",
                table: "codigo_recuperacao");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "grupo");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "despesa");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "parcela",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "despesa",
                newName: "Status");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:status_despesa", "ATIVA,QUITADA,CANCELADA")
                .Annotation("Npgsql:Enum:status_parcela", "PENDENTE,PAGO,ATRASADO");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "parcela",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "despesa",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_codigo_recuperacao",
                table: "codigo_recuperacao",
                column: "id_codigo");
        }
    }
}
