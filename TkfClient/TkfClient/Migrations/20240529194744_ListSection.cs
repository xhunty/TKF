using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TkfClient.Migrations
{
    /// <inheritdoc />
    public partial class ListSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ListSection",
                table: "Share",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListSection",
                table: "Share");
        }
    }
}
