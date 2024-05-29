using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TkfClient.Migrations
{
    /// <inheritdoc />
    public partial class Remove_ISIN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Isin",
                table: "Candle");

            migrationBuilder.AlterColumn<string>(
                name: "Uid",
                table: "Candle",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Uid",
                table: "Candle",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "Candle",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
