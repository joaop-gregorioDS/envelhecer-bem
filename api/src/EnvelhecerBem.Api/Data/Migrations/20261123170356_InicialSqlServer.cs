using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvelhecerBem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lembretes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DataHoraUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lembretes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosDiarios",
                columns: table => new
                {
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    HorasSono = table.Column<double>(type: "float", nullable: false),
                    Passos = table.Column<int>(type: "int", nullable: false),
                    CoposDeAgua = table.Column<int>(type: "int", nullable: false),
                    Humor = table.Column<int>(type: "int", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosDiarios", x => x.Data);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lembretes");

            migrationBuilder.DropTable(
                name: "RegistrosDiarios");
        }
    }
}
