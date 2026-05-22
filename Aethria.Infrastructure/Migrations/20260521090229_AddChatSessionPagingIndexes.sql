START TRANSACTION;
DROP INDEX ix_chat_sessions_mentor_id;

DROP INDEX ix_chat_sessions_resource_id;

CREATE INDEX ix_chat_sessions_general_user_id_updated_at ON chat_sessions (user_id, updated_at DESC) WHERE mentor_id IS NULL AND resource_id IS NULL;

CREATE INDEX ix_chat_sessions_mentor_id_user_id_updated_at ON chat_sessions (mentor_id, user_id, updated_at DESC);

CREATE INDEX ix_chat_sessions_resource_id_user_id_updated_at ON chat_sessions (resource_id, user_id, updated_at DESC);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260521090229_AddChatSessionPagingIndexes', '10.0.8');

COMMIT;

