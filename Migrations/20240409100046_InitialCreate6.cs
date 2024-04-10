using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortfolyoWebSite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "LinkIcons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkIcons_ContactId",
                table: "LinkIcons",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkIcons_Contacts_ContactId",
                table: "LinkIcons",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "ContactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkIcons_Contacts_ContactId",
                table: "LinkIcons");

            migrationBuilder.DropIndex(
                name: "IX_LinkIcons_ContactId",
                table: "LinkIcons");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "LinkIcons");
        }
    }
}
