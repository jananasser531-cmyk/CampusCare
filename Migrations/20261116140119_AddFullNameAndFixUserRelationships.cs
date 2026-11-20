using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCare.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameAndFixUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketAttachments_AspNetUsers_ApplicationUserId",
                table: "TicketAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_AspNetUsers_ApplicationUserId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_ApplicationUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketStatusHistories_AspNetUsers_ApplicationUserId",
                table: "TicketStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_TicketStatusHistories_ApplicationUserId",
                table: "TicketStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ApplicationUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_ApplicationUserId",
                table: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_TicketAttachments_ApplicationUserId",
                table: "TicketAttachments");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "TicketStatusHistories");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "TicketAttachments");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "TicketStatusHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "TicketComments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "TicketAttachments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatusHistories_ApplicationUserId",
                table: "TicketStatusHistories",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ApplicationUserId",
                table: "Tickets",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_ApplicationUserId",
                table: "TicketComments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAttachments_ApplicationUserId",
                table: "TicketAttachments",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketAttachments_AspNetUsers_ApplicationUserId",
                table: "TicketAttachments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_AspNetUsers_ApplicationUserId",
                table: "TicketComments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_ApplicationUserId",
                table: "Tickets",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketStatusHistories_AspNetUsers_ApplicationUserId",
                table: "TicketStatusHistories",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
