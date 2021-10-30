#!/bin/bash

# quick exit with error message
bail() {
    echo "###" $1
    exit 1;
}

_ENV=".env"

# move to dir if passed
if [[! -z $1 ]]; then
    if [[ -e $1 ]]; then
        echo "===> Using .env file '$1'"
        _ENV=$1
    else
        bail "File does not exist '$1'"
    fi
fi

# declare docker names
_SBU_VOLUME='sbu-bot-volume'
_SBU_NETWORK='sbu-bot-network'
_SBU_CONTAINER='sbu-bot'
_SBU_IMAGE='erxkk/sbu-bot:latest'
_SBU_DATABASE='postgres'

# check if volume exists
if [[ -z "$(docker volume ls | grep $_SBU_VOLUME)" ]]; then
    echo "===> Creating volume '$_SBU_VOLUME'"
    docker volume create $_SBU_VOLUME || bail "Could not create $_SBU_VOLUME"
fi
_VOLUME_MOUNT=$(docker volume inspect -f '{{.Mountpoint}}' $_SBU_VOLUME)
echo "===> Found volume mount '$_VOLUME_MOUNT'"

# check if db is running
if [[ -z "$(docker ps | grep $_SBU_DATABASE)" ]]; then
    bail "===> Could not find db '$_SBU_DATABASE'"
fi

# check if network exists
if [[ -z "$(docker network ls | grep $_SBU_NETWORK)" ]]; then
    echo "===> Creating network '$_SBU_NETWORK'"
    docker network create $_SBU_NETWORK || bail "Could not create $_SBU_NETWORK"

    echo "===> Connecting '$_SBU_DATABASE' to '$_SBU_NETWORK'"
    docker network connect $_SBU_NETWORK $_SBU_DATABASE || bail "Could not connect $_SBU_DATABASE to $_SBU_NETWORK"
fi

# if container exists stop and remove
if [[ ! -z "$(docker ps -a | grep $_SBU_CONTAINER)" ]]; then
    # stop container
    echo "===> Stopping '$_SBU_CONTAINER'"
    docker stop $_SBU_CONTAINER || bail "Could not stop $_SBU_CONTAINER"

    # remove container
    echo "===> Removing '$_SBU_CONTAINER'"
    docker container rm $_SBU_CONTAINER || bail "Could not remove $_SBU_CONTAINER"

    # pull newest image
    echo "===> Pulling '$_SBU_IMAGE'"
    docker pull $_SBU_IMAGE || bail "Could not pull $_SBU_IMAGE"
fi

# restart container
echo "===> Starting '$SBU_BOT_CONTAINER'"
docker run -d \
    --name $_SBU_CONTAINER \
    --env-file $_ENV \
    --volume $_SBU_VOLUME:/volume \
    --network $_SBU_NETWORK \
    $_SBU_IMAGE \
        || bail "Could not run $_SBU_CONTAINER"

