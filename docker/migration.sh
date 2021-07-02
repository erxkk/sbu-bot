#!/bin/bash

#previous_image_sha=$(docker inspect sbu-bot | grep 'Image": "sha' | sed 's/\s*"Image": "sha256:\(.*\?\)",/\1/g')
docker stop sbu-bot && docker container rm sbu-bot && docker image rm erxkk/sbu-bot:latest

docker pull erxkk/sbu-bot:latest
docker run -d \
  --name sbu-bot \
  --env-file /home/pi/sbu-bot/.env \
  --volume sbu-bot-volume:/volume \
  --network sbu-bot-postgres-bridge \
  erxkk/sbu-bot:latest
