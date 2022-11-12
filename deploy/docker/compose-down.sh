#!/bin/bash

docker-compose down

docker volume rm dev-seq_data
docker volume rm dev-sql-server_data