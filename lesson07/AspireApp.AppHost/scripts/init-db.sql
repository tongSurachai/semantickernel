DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'myuser') THEN
    CREATE ROLE myuser WITH LOGIN PASSWORD 'mypassword';
END IF;
END $$;

ALTER ROLE myuser CREATEDB;
