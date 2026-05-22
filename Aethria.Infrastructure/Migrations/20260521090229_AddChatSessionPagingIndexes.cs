using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSessionPagingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_chat_sessions_mentor_id",
                table: "chat_sessions");

            migrationBuilder.DropIndex(
                name: "ix_chat_sessions_resource_id",
                table: "chat_sessions");

            migrationBuilder.CreateIndex(
                name: "ix_chat_sessions_general_user_id_updated_at",
                table: "chat_sessions",
                columns: new[] { "user_id", "updated_at" },
                descending: new[] { false, true },
                filter: "mentor_id IS NULL AND resource_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_chat_sessions_mentor_id_user_id_updated_at",
                table: "chat_sessions",
                columns: new[] { "mentor_id", "user_id", "updated_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_chat_sessions_resource_id_user_id_updated_at",
                table: "chat_sessions",
                columns: new[] { "resource_id", "user_id", "updated_at" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_chat_sessions_general_user_id_updated_at",
                table: "chat_sessions");

            migrationBuilder.DropIndex(
                name: "ix_chat_sessions_mentor_id_user_id_updated_at",
                table: "chat_sessions");

            migrationBuilder.DropIndex(
                name: "ix_chat_sessions_resource_id_user_id_updated_at",
                table: "chat_sessions");

            migrationBuilder.CreateIndex(
                name: "ix_chat_sessions_mentor_id",
                table: "chat_sessions",
                column: "mentor_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_sessions_resource_id",
                table: "chat_sessions",
                column: "resource_id");
        }
    }
}
