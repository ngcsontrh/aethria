using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethria.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260519090000_AddApiKeyExpirationCleanupJob")]
    public partial class AddApiKeyExpirationCleanupJob : Migration
    {
        private const string JobName = "aethria_cleanup_expired_api_keys";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                CREATE EXTENSION IF NOT EXISTS pg_cron;

                SELECT cron.unschedule(jobid)
                FROM cron.job
                WHERE jobname = '{JobName}';

                SELECT cron.schedule(
                    '{JobName}',
                    '30 seconds',
                    $job$
                    UPDATE api_keys
                    SET status = 'revoked',
                        revoked_at = COALESCE(revoked_at, now()),
                        updated_at = now()
                    WHERE status = 'active'
                      AND expires_at < now();
                    $job$);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                SELECT cron.unschedule(jobid)
                FROM cron.job
                WHERE jobname = '{JobName}';
                """);
        }
    }
}
