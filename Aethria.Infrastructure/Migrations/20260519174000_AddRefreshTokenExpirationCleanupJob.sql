START TRANSACTION;

CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.unschedule(jobid)
FROM cron.job
WHERE jobname = 'aethria_revoke_expired_refresh_tokens';

SELECT cron.schedule(
    'aethria_revoke_expired_refresh_tokens',
    '30 seconds',
    $job$
    UPDATE refresh_tokens
    SET status = 'revoked',
        revoked_at = COALESCE(revoked_at, now()),
        updated_at = now()
    WHERE status = 'active'
      AND expires_at < now();
    $job$);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260519174000_AddRefreshTokenExpirationCleanupJob', '10.0.8');

COMMIT;

