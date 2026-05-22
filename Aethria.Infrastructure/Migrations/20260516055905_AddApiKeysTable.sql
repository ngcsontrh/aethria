START TRANSACTION;

CREATE TABLE api_keys (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(100) NOT NULL,
    token_hash character varying(88) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    status character varying(20) NOT NULL,
    revoked_at timestamp with time zone NULL,
    last_four_chars character varying(4) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_api_keys PRIMARY KEY (id),
    CONSTRAINT fk_api_keys_asp_net_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX ix_api_keys_expires_at_status ON api_keys (expires_at, status);

CREATE UNIQUE INDEX ix_api_keys_token_hash ON api_keys (token_hash);

CREATE INDEX ix_api_keys_user_id_status ON api_keys (user_id, status);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260516055905_AddApiKeysTable', '10.0.7');

COMMIT;

