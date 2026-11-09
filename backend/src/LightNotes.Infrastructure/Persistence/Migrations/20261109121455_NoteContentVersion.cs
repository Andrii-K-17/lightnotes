using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LightNotes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NoteContentVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "notes",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AddColumn<int>(
                name: "content_version",
                table: "notes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_version",
                table: "notes");

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "notes",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json");
        }
    }
}
