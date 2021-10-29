# sbu-bot

A personal project for a discord server helper. The project is mainly used to simplify server management of the sbu
discord server and to learn current technologies for application development & deployment.

I don't really expect people to fork/pr/contribute, but if you want to the docker setup is very specific to my home
setup, the bot is hosted on docker container running on a Raspberry Pi 4 on Raspberry Pi OS. The `docker/template.env`
file contains the required environment variables for the bot to be run.

Constructive criticism is welcome.

## Features

| Feature | Initially planned | Available outside of SBU |
|:---|:---:|:---:|
| generic interactive help menu | • | • |
| archival of messages/pins | • | • |
| color role creation and management | • | • |
| permission requests for specific channels | • |   |
| tags |   | • |
| auto responses |   | • |
| simple reminders |   | • |
| reflection based inspection |   | Bot owner only |
| runtime code evaluation |   | Bot owner only |
| object inspection of database entries |   | Server admin only |

Features that weren't initially planned were added as exercises and learning experiences.

## External Libraries

Big thanks to the lads for creating the following tools:

* [Disqord](https://github.com/quahu/disqord)
* [HumanTimeParser](https://github.com/Zackattak01/HumanTimeParser)

## Things I learned on the way

* [x] building and migrating docker images for a linux/armv7 host
* [x] using git submodules
* [x] logging with serilog
* [x] dependency injection with the .NET Generic Host
* [ ] writing good code