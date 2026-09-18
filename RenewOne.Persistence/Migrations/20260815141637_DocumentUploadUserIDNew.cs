using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentUploadUserIDNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedBy",
                table: "Documents",
                newName: "UploadedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedUserId",
                table: "Documents",
                column: "UploadedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_UploadedUserId",
                table: "Documents",
                column: "UploadedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_UploadedUserId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_UploadedUserId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UploadedUserId",
                table: "Documents",
                newName: "UploadedBy");
        }
    }
}
