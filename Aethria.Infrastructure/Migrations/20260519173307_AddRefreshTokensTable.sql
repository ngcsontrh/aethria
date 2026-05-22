START TRANSACTION;

CREATE TABLE refresh_tokens (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    token_hash character varying(88) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    status character varying(20) NOT NULL,
    revoked_at timestamp with time zone NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
    CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX ix_refresh_tokens_expires_at_status ON refresh_tokens (expires_at, status);

CREATE UNIQUE INDEX ix_refresh_tokens_token_hash ON refresh_tokens (token_hash);

CREATE INDEX ix_refresh_tokens_user_id_status ON refresh_tokens (user_id, status);

DELETE FROM user_tokens
WHERE login_provider = 'Aethria'
  AND name = 'RefreshToken';

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260519173307_AddRefreshTokensTable', '10.0.8');

COMMIT;

