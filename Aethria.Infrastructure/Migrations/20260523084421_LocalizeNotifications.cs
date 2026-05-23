using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocalizeNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM notifications;");

            migrationBuilder.DropIndex(
                name: "ix_notifications_is_read",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "message",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "name",
                table: "notifications");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "data",
                table: "notifications",
                type: "jsonb",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read_created_at",
                table: "notifications",
                columns: new[] { "user_id", "is_read", "created_at" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notifications_user_id_is_read_created_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "data",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "type",
                table: "notifications");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "notifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_is_read",
                table: "notifications",
                column: "is_read");
        }
    }
}
