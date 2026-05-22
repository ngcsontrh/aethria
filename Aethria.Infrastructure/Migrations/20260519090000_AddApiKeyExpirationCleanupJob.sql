START TRANSACTION;

CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.unschedule(jobid)
FROM cron.job
WHERE jobname = 'aethria_cleanup_expired_api_keys';

SELECT cron.schedule(
    'aethria_cleanup_expired_api_keys',
    '30 seconds',
    $job$
    UPDATE api_keys
    SET status = 'revoked',
        revoked_at = COALESCE(revoked_at, now()),
        updated_at = now()
    WHERE status = 'active'
      AND expires_at < now();
    $job$);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260519090000_AddApiKeyExpirationCleanupJob', '10.0.7');

COMMIT;
