#!/bin/bash

# Запуск Redis-сервера в фоновом режиме
redis-server --daemonize yes

# Сидинг данных в Redis в JSON
redis-cli <<EOF
SET regions:1 '{"Id": 1, "Name": "Moscow", "Storage": {"Latitude": 55.700608, "Longitude": 37.588771}}'
SET regions:2 '{"Id": 2, "Name": "StPetersburg", "Storage": {"Latitude": 59.910008, "Longitude": 30.351155}}'
SET regions:3 '{"Id": 3, "Name": "Novosibirsk", "Storage": {"Latitude": 54.973413, "Longitude": 82.856013}}'
EOF

# Выключение сервера Redis
redis-cli shutdown

# Запуск сервера Redis
redis-server