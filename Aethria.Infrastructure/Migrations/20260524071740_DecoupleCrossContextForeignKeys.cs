using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleCrossContextForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_api_keys_asp_net_users_user_id",
                table: "api_keys");

            migrationBuilder.DropForeignKey(
                name: "fk_chat_sessions_resources_resource_id",
                table: "chat_sessions");

            migrationBuilder.DropForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_api_keys_asp_net_users_user_id",
                table: "api_keys",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_chat_sessions_resources_resource_id",
                table: "chat_sessions",
                column: "resource_id",
                principalTable: "resources",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_refresh_tokens_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
