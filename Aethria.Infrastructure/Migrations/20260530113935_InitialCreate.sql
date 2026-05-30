CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;
CREATE TABLE api_keys (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(100) NOT NULL,
    token_hash character varying(88) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    status character varying(20) NOT NULL,
    revoked_at timestamp with time zone,
    last_four_chars character varying(4) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_api_keys PRIMARY KEY (id)
);

CREATE TABLE chat_sessions (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    mentor_id uuid,
    resource_id uuid,
    name character varying(255) NOT NULL,
    description text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_chat_sessions PRIMARY KEY (id)
);

CREATE TABLE mentors (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(255) NOT NULL,
    description text NOT NULL,
    instruction text NOT NULL,
    tools jsonb NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_mentors PRIMARY KEY (id)
);

CREATE TABLE notifications (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    type character varying(255) NOT NULL,
    data jsonb NOT NULL,
    is_read boolean NOT NULL DEFAULT FALSE,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_notifications PRIMARY KEY (id)
);

CREATE TABLE quiz_submissions (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    quiz_id uuid NOT NULL,
    quiz_version_id uuid NOT NULL,
    score integer NOT NULL,
    total_questions integer NOT NULL,
    is_passed boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_quiz_submissions PRIMARY KEY (id),
    CONSTRAINT ck_quiz_submissions_score_valid_range CHECK ("score" >= 0 AND "score" <= "total_questions")
);

CREATE TABLE quizzes (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(255) NOT NULL,
    description text,
    resource_id uuid,
    current_version_number integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_quizzes PRIMARY KEY (id)
);

CREATE TABLE refresh_tokens (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    token_hash character varying(88) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    status character varying(20) NOT NULL,
    revoked_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id)
);

CREATE TABLE resources (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(255) NOT NULL,
    description text,
    file_uri character varying(1024) NOT NULL,
    file_type character varying(100) NOT NULL,
    file_size bigint NOT NULL,
    content text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_resources PRIMARY KEY (id),
    CONSTRAINT ck_resources_file_size_positive CHECK ("file_size" >= 0)
);

CREATE TABLE roadmaps (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    name character varying(255) NOT NULL,
    description text,
    content text NOT NULL,
    mermaid text,
    resource_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_roadmaps PRIMARY KEY (id)
);

CREATE TABLE roles (
    id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    name character varying(256),
    normalized_name character varying(256),
    concurrency_stamp text,
    CONSTRAINT pk_roles PRIMARY KEY (id)
);

CREATE TABLE users (
    id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    user_name character varying(256),
    normalized_user_name character varying(256),
    email character varying(256),
    normalized_email character varying(256),
    email_confirmed boolean NOT NULL,
    password_hash text,
    security_stamp text,
    concurrency_stamp text,
    phone_number text,
    phone_number_confirmed boolean NOT NULL,
    two_factor_enabled boolean NOT NULL,
    lockout_end timestamp with time zone,
    lockout_enabled boolean NOT NULL,
    access_failed_count integer NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
);

CREATE TABLE chat_messages (
    id uuid NOT NULL,
    session_id uuid NOT NULL,
    role character varying(50) NOT NULL,
    content text NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_chat_messages PRIMARY KEY (id),
    CONSTRAINT fk_chat_messages_chat_sessions_session_id FOREIGN KEY (session_id) REFERENCES chat_sessions (id) ON DELETE CASCADE
);

CREATE TABLE submission_answers (
    id uuid NOT NULL,
    quiz_submission_id uuid NOT NULL,
    question_snapshot_id uuid NOT NULL,
    selected_option_id uuid NOT NULL,
    is_correct boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_submission_answers PRIMARY KEY (id),
    CONSTRAINT fk_submission_answers_quiz_submissions_quiz_submission_id FOREIGN KEY (quiz_submission_id) REFERENCES quiz_submissions (id) ON DELETE CASCADE
);

CREATE TABLE quiz_questions (
    id uuid NOT NULL,
    quiz_id uuid NOT NULL,
    text text NOT NULL,
    correct_option_id uuid NOT NULL,
    explanation text NOT NULL,
    order_index integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_quiz_questions PRIMARY KEY (id),
    CONSTRAINT fk_quiz_questions_quizzes_quiz_id FOREIGN KEY (quiz_id) REFERENCES quizzes (id) ON DELETE CASCADE
);

CREATE TABLE quiz_versions (
    id uuid NOT NULL,
    quiz_id uuid NOT NULL,
    version_number integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_quiz_versions PRIMARY KEY (id),
    CONSTRAINT fk_quiz_versions_quizzes_quiz_id FOREIGN KEY (quiz_id) REFERENCES quizzes (id) ON DELETE CASCADE
);

CREATE TABLE role_claims (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    role_id uuid NOT NULL,
    claim_type text,
    claim_value text,
    CONSTRAINT pk_role_claims PRIMARY KEY (id),
    CONSTRAINT fk_role_claims_roles_role_id FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE
);

CREATE TABLE user_claims (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    user_id uuid NOT NULL,
    claim_type text,
    claim_value text,
    CONSTRAINT pk_user_claims PRIMARY KEY (id),
    CONSTRAINT fk_user_claims_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE user_logins (
    login_provider text NOT NULL,
    provider_key text NOT NULL,
    provider_display_name text,
    user_id uuid NOT NULL,
    CONSTRAINT pk_user_logins PRIMARY KEY (login_provider, provider_key),
    CONSTRAINT fk_user_logins_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE user_roles (
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_roles_role_id FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE user_tokens (
    user_id uuid NOT NULL,
    login_provider text NOT NULL,
    name text NOT NULL,
    value text,
    CONSTRAINT pk_user_tokens PRIMARY KEY (user_id, login_provider, name),
    CONSTRAINT fk_user_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE question_options (
    id uuid NOT NULL,
    quiz_question_id uuid NOT NULL,
    text text NOT NULL,
    order_index integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_question_options PRIMARY KEY (id),
    CONSTRAINT fk_question_options_quiz_questions_quiz_question_id FOREIGN KEY (quiz_question_id) REFERENCES quiz_questions (id) ON DELETE CASCADE
);

CREATE TABLE question_snapshots (
    id uuid NOT NULL,
    quiz_version_id uuid NOT NULL,
    original_question_id uuid NOT NULL,
    text text NOT NULL,
    explanation text NOT NULL,
    correct_option_id uuid NOT NULL,
    order_index integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_question_snapshots PRIMARY KEY (id),
    CONSTRAINT fk_question_snapshots_quiz_versions_quiz_version_id FOREIGN KEY (quiz_version_id) REFERENCES quiz_versions (id) ON DELETE CASCADE
);

CREATE TABLE question_option_snapshots (
    id uuid NOT NULL,
    question_snapshot_id uuid NOT NULL,
    original_option_id uuid NOT NULL,
    text text NOT NULL,
    order_index integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_question_option_snapshots PRIMARY KEY (id),
    CONSTRAINT fk_question_option_snapshots_question_snapshots_question_snaps FOREIGN KEY (question_snapshot_id) REFERENCES question_snapshots (id) ON DELETE CASCADE
);

CREATE INDEX ix_api_keys_expires_at_status ON api_keys (expires_at, status);

CREATE UNIQUE INDEX ix_api_keys_token_hash ON api_keys (token_hash);

CREATE INDEX ix_api_keys_user_id_status ON api_keys (user_id, status);

CREATE INDEX ix_chat_messages_session_id ON chat_messages (session_id);

CREATE INDEX ix_chat_sessions_general_user_id_updated_at ON chat_sessions (user_id, updated_at DESC) WHERE mentor_id IS NULL AND resource_id IS NULL;

CREATE INDEX ix_chat_sessions_mentor_id_user_id_updated_at ON chat_sessions (mentor_id, user_id, updated_at DESC);

CREATE INDEX ix_chat_sessions_resource_id_user_id_updated_at ON chat_sessions (resource_id, user_id, updated_at DESC);

CREATE INDEX ix_notifications_user_id_is_read_created_at ON notifications (user_id, is_read, created_at DESC);

CREATE INDEX ix_question_option_snapshots_question_snapshot_id ON question_option_snapshots (question_snapshot_id);

CREATE INDEX ix_question_options_quiz_question_id ON question_options (quiz_question_id);

CREATE INDEX ix_question_snapshots_quiz_version_id ON question_snapshots (quiz_version_id);

CREATE INDEX ix_quiz_questions_quiz_id ON quiz_questions (quiz_id);

CREATE INDEX ix_quiz_submissions_quiz_id ON quiz_submissions (quiz_id);

CREATE INDEX ix_quiz_submissions_quiz_version_id ON quiz_submissions (quiz_version_id);

CREATE UNIQUE INDEX ix_quiz_versions_quiz_id_version_number ON quiz_versions (quiz_id, version_number);

CREATE INDEX ix_quizzes_resource_id ON quizzes (resource_id);

CREATE INDEX ix_refresh_tokens_expires_at_status ON refresh_tokens (expires_at, status);

CREATE UNIQUE INDEX ix_refresh_tokens_token_hash ON refresh_tokens (token_hash);

CREATE INDEX ix_refresh_tokens_user_id_status ON refresh_tokens (user_id, status);

CREATE INDEX ix_roadmaps_resource_id ON roadmaps (resource_id);

CREATE INDEX ix_role_claims_role_id ON role_claims (role_id);

CREATE UNIQUE INDEX ix_roles_normalized_name ON roles (normalized_name);

CREATE INDEX ix_submission_answers_question_snapshot_id ON submission_answers (question_snapshot_id);

CREATE INDEX ix_submission_answers_quiz_submission_id ON submission_answers (quiz_submission_id);

CREATE INDEX ix_user_claims_user_id ON user_claims (user_id);

CREATE INDEX ix_user_logins_user_id ON user_logins (user_id);

CREATE INDEX ix_user_roles_role_id ON user_roles (role_id);

CREATE INDEX ix_users_normalized_email ON users (normalized_email);

CREATE UNIQUE INDEX ix_users_normalized_user_name ON users (normalized_user_name);

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260530113935_InitialCreate', '10.0.8');

COMMIT;

