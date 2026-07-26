using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModulePermissionTableRenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserModulePermission_Users_UserId",
                table: "UserModulePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserModulePermission",
                table: "UserModulePermission");

            migrationBuilder.RenameTable(
                name: "UserModulePermission",
                newName: "UserModulePermissions");

            migrationBuilder.RenameIndex(
                name: "IX_UserModulePermission_UserId",
                table: "UserModulePermissions",
                newName: "IX_UserModulePermissions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserModulePermissions",
                table: "UserModulePermissions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserModulePermissions_Users_UserId",
                table: "UserModulePermissions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserModulePermissions_Users_UserId",
                table: "UserModulePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserModulePermissions",
                table: "UserModulePermissions");

            migrationBuilder.RenameTable(
                name: "UserModulePermissions",
                newName: "UserModulePermission");

            migrationBuilder.RenameIndex(
                name: "IX_UserModulePermissions_UserId",
                table: "UserModulePermission",
                newName: "IX_UserModulePermission_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserModulePermission",
                table: "UserModulePermission",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserModulePermission_Users_UserId",
                table: "UserModulePermission",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
