#!/bin/sh

set -e  # Exit on error

echo "📌 Installing dependencies..."
apt-get update && apt-get install -y wget gnupg

echo "📌 Adding PostgreSQL repository..."
echo 'deb http://apt.postgresql.org/pub/repos/apt bookworm-pgdg main' > /etc/apt/sources.list.d/pgdg.list
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add -
apt-get update

echo "📌 Installing pgvector..."
PG_VERSION=$(ls /usr/lib/postgresql/)
apt-get install -y postgresql-$PG_VERSION-pgvector

echo "📌 Fixing permissions..."
chown -R postgres:postgres /var/lib/postgresql

echo "📌 Initializing database if needed..."
if [ -z "$(ls -A /var/lib/postgresql/data)" ]; then
    su - postgres -c "/usr/lib/postgresql/$PG_VERSION/bin/initdb -D /var/lib/postgresql/data"
else
    echo "✅ Database already initialized."
fi

echo "📌 Configuring PostgreSQL..."
sed -i '/^listen_addresses/d' /var/lib/postgresql/data/postgresql.conf
echo "listen_addresses = '*'" >> /var/lib/postgresql/data/postgresql.conf
echo "host all all 0.0.0.0/0 trust" >> /var/lib/postgresql/data/pg_hba.conf

echo "📌 Starting PostgreSQL in the background..."
su - postgres -c "/usr/lib/postgresql/$PG_VERSION/bin/postgres -D /var/lib/postgresql/data" &

sleep 5  # Give PostgreSQL some time to start

echo "📌 Running database initialization script..."
su - postgres -c "psql -d postgres -f /scripts/init-db.sql"

echo "✅ PostgreSQL setup complete, keeping it running in foreground..."

# 🚀 Start PostgreSQL as the foreground process
exec su - postgres -c "/usr/lib/postgresql/$PG_VERSION/bin/postgres -D /var/lib/postgresql/data"
