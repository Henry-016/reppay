using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RepPay.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:status_despesa", "ATIVA,QUITADA,CANCELADA")
                .Annotation("Npgsql:Enum:status_parcela", "PENDENTE,PAGO,ATRASADO");

            migrationBuilder.CreateTable(
                name: "codigo_recuperacao",
                columns: table => new
                {
                    id_codigo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    data_expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    codigo_usado = table.Column<bool>(type: "boolean", nullable: false),
                    tentativas = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_codigo_recuperacao", x => x.id_codigo);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    senha = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuario_pkey", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "grupo",
                columns: table => new
                {
                    id_grupo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_acesso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    imagem_banner = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    id_admin = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("grupo_pkey", x => x.id_grupo);
                    table.ForeignKey(
                        name: "fk_admin_grupo",
                        column: x => x.id_admin,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "despesa",
                columns: table => new
                {
                    id_despesa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_cadastro = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    icone = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    id_grupo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("despesa_pkey", x => x.id_despesa);
                    table.ForeignKey(
                        name: "fk_despesa_grupo",
                        column: x => x.id_grupo,
                        principalTable: "grupo",
                        principalColumn: "id_grupo");
                });

            migrationBuilder.CreateTable(
                name: "pertence",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_grupo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pertence_pkey", x => new { x.id_usuario, x.id_grupo });
                    table.ForeignKey(
                        name: "fk_pertence_grupo",
                        column: x => x.id_grupo,
                        principalTable: "grupo",
                        principalColumn: "id_grupo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pertence_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parcela",
                columns: table => new
                {
                    id_parcela = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    data_pagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_despesa = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("parcela_pkey", x => x.id_parcela);
                    table.ForeignKey(
                        name: "fk_parcela_despesa",
                        column: x => x.id_despesa,
                        principalTable: "despesa",
                        principalColumn: "id_despesa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parcela_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_despesa_id_grupo",
                table: "despesa",
                column: "id_grupo");

            migrationBuilder.CreateIndex(
                name: "grupo_codigo_acesso_key",
                table: "grupo",
                column: "codigo_acesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grupo_id_admin",
                table: "grupo",
                column: "id_admin");

            migrationBuilder.CreateIndex(
                name: "idx_parcela_despesa",
                table: "parcela",
                column: "id_despesa");

            migrationBuilder.CreateIndex(
                name: "parcela_id_usuario_id_despesa_key",
                table: "parcela",
                columns: new[] { "id_usuario", "id_despesa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pertence_id_grupo",
                table: "pertence",
                column: "id_grupo");

            migrationBuilder.CreateIndex(
                name: "usuario_email_key",
                table: "usuario",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codigo_recuperacao");

            migrationBuilder.DropTable(
                name: "parcela");

            migrationBuilder.DropTable(
                name: "pertence");

            migrationBuilder.DropTable(
                name: "despesa");

            migrationBuilder.DropTable(
                name: "grupo");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
