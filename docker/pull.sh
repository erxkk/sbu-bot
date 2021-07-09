#!/bin/bash

docker pull erxkk/sbu-bot:latest
docker run -d \
  --name sbu-bot \
  --env-file /home/pi/sbu-bot/.env \
  --volume sbu-bot-volume:/volume \
  --network sbu-bot-postgres-bridge \
  erxkk/sbu-bot:latest
