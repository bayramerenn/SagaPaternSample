using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockAPI.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Stocks",
                columns: new[] { "Id", "Count", "CreatedDate", "ProductId" },
                values: new object[,]
                {
                    { 1, 100, new DateTime(2022, 12, 10, 0, 26, 4, 336, DateTimeKind.Local).AddTicks(1209), 1 },
                    { 2, 100, new DateTime(2022, 12, 10, 0, 26, 4, 336, DateTimeKind.Local).AddTicks(1210), 2 },
                    { 3, 100, new DateTime(2022, 12, 10, 0, 26, 4, 336, DateTimeKind.Local).AddTicks(1211), 3 },
                    { 4, 100, new DateTime(2022, 12, 10, 0, 26, 4, 336, DateTimeKind.Local).AddTicks(1212), 4 },
                    { 5, 100, new DateTime(2022, 12, 10, 0, 26, 4, 336, DateTimeKind.Local).AddTicks(1213), 5 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stocks");
        }
    }
}
