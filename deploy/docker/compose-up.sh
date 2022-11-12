#!/bin/bash

if [[ "$(docker images -q authclient:latest 2> /dev/null)" != "" ]]; then
  docker image rm authclient
fi

docker build ../../ -t authclient

# Create the needed volumns for the services used by the Wiki examples:
docker volume create --name=dev-seq_data
docker volume create --name=dev-sql-server_data


rm -rfd ./sql-server/scripts
cp -r ../sql-scripts ./sql-server/scripts


# Run all the needed services within Docker
docker-compose up -d

rm -rfd ./sql-server/scripts