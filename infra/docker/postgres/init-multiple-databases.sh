#!/bin/bash
set -e

if [ -z "$POSTGRES_MULTIPLE_DATABASES" ]; then
  echo "No additional databases requested."
  exit 0
fi

create_database() {
  local database
  database="$1"

  echo "Creating database '$database'"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres <<-EOSQL
    SELECT 'CREATE DATABASE "$database"'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$database')\gexec
EOSQL
}

IFS=',' read -ra database_array <<< "$POSTGRES_MULTIPLE_DATABASES"

for database in "${database_array[@]}"; do
  trimmed="$(echo "$database" | xargs)"

  if [ -n "$trimmed" ]; then
    create_database "$trimmed"
  fi
done

echo "Additional databases are ready."

