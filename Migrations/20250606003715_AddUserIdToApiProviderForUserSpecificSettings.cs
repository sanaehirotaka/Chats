using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chats.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToApiProviderForUserSpecificSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiProviders",
                table: "ApiProviders");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ApiProviders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiProviders",
                table: "ApiProviders",
                columns: new[] { "ProviderName", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApiProviders_UserId",
                table: "ApiProviders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiProviders_AspNetUsers_UserId",
                table: "ApiProviders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiProviders_AspNetUsers_UserId",
                table: "ApiProviders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiProviders",
                table: "ApiProviders");

            migrationBuilder.DropIndex(
                name: "IX_ApiProviders_UserId",
                table: "ApiProviders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApiProviders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiProviders",
                table: "ApiProviders",
                column: "ProviderName");
        }
    }
}
