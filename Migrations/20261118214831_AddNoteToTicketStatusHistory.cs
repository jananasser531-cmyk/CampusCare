using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCare.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteToTicketStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "TicketStatusHistories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "TicketStatusHistories");
        }
    }
}
