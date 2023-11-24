using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TkfClient.Migrations
{
    /// <inheritdoc />
    public partial class fix_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "Candle",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uid",
                table: "Candle");
        }
    }
}
