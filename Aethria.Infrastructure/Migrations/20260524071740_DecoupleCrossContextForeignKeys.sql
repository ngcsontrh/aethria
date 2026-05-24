START TRANSACTION;
ALTER TABLE api_keys DROP CONSTRAINT fk_api_keys_asp_net_users_user_id;

ALTER TABLE chat_sessions DROP CONSTRAINT fk_chat_sessions_resources_resource_id;

ALTER TABLE refresh_tokens DROP CONSTRAINT fk_refresh_tokens_users_user_id;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260524071740_DecoupleCrossContextForeignKeys', '10.0.8');

COMMIT;

