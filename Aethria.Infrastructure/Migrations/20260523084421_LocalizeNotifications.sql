START TRANSACTION;
DELETE FROM notifications;

DROP INDEX ix_notifications_is_read;

ALTER TABLE notifications DROP COLUMN message;

ALTER TABLE notifications DROP COLUMN name;

ALTER TABLE notifications ADD type character varying(255) NOT NULL;

ALTER TABLE notifications ADD data jsonb NOT NULL;

CREATE INDEX ix_notifications_user_id_is_read_created_at ON notifications (user_id, is_read, created_at DESC);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260523084421_LocalizeNotifications', '10.0.8');

COMMIT;

