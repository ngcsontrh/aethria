START TRANSACTION;

DROP TABLE resource_chunks;

INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20260530000000_DropResourceChunks', '10.0.8');

COMMIT;
