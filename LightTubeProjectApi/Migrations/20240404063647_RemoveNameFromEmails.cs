using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LightTubeProjectApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNameFromEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Emails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Emails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
