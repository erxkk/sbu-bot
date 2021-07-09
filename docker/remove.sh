#!/bin/bash

#previous_image_sha=$(docker inspect sbu-bot | grep 'Image": "sha' | sed 's/\s*"Image": "sha256:\(.*\?\)",/\1/g')
docker stop sbu-bot && docker container rm sbu-bot && docker image rm erxkk/sbu-bot:latest