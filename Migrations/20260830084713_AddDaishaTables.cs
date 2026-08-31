using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TireTraceabilityDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddDaishaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================
            // CREATE TABLE DAISHAS
            // =========================================================

            migrationBuilder.CreateTable(
                name: "Daishas",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    DaishaCode = table.Column<string>(
                        type: "longtext",
                        nullable: false)
                        .Annotation(
                            "MySql:CharSet",
                            "utf8mb4"),

                    ComputerName = table.Column<string>(
                        type: "longtext",
                        nullable: false)
                        .Annotation(
                            "MySql:CharSet",
                            "utf8mb4"),

                    OperatorName = table.Column<string>(
                        type: "longtext",
                        nullable: false)
                        .Annotation(
                            "MySql:CharSet",
                            "utf8mb4"),

                    TotalTires = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false),

                    Status = table.Column<string>(
                        type: "longtext",
                        nullable: false)
                        .Annotation(
                            "MySql:CharSet",
                            "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Daishas",
                        x => x.Id);
                })
                .Annotation(
                    "MySql:CharSet",
                    "utf8mb4");


            // =========================================================
            // CREATE TABLE DAISHA TIRES
            // =========================================================

            migrationBuilder.CreateTable(
                name: "DaishaTires",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    DaishaId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    TireId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Sequence = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_DaishaTires",
                        x => x.Id);


                    // -------------------------------------------------
                    // DAISHATIRE -> DAISHA
                    // -------------------------------------------------

                    table.ForeignKey(
                        name: "FK_DaishaTires_Daishas_DaishaId",
                        column: x => x.DaishaId,
                        principalTable: "Daishas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);


                    // -------------------------------------------------
                    // DAISHATIRE -> TIRE
                    // -------------------------------------------------

                    table.ForeignKey(
                        name: "FK_DaishaTires_Tires_TireId",
                        column: x => x.TireId,
                        principalTable: "Tires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation(
                    "MySql:CharSet",
                    "utf8mb4");


            // =========================================================
            // INDEX DAISHA ID
            // =========================================================

            migrationBuilder.CreateIndex(
                name: "IX_DaishaTires_DaishaId",
                table: "DaishaTires",
                column: "DaishaId");


            // =========================================================
            // INDEX TIRE ID
            // =========================================================

            migrationBuilder.CreateIndex(
                name: "IX_DaishaTires_TireId",
                table: "DaishaTires",
                column: "TireId");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =========================================================
            // HAPUS DAISHA TIRES
            // =========================================================

            migrationBuilder.DropTable(
                name: "DaishaTires");


            // =========================================================
            // HAPUS DAISHAS
            // =========================================================

            migrationBuilder.DropTable(
                name: "Daishas");
        }
    }
}