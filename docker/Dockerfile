FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime
WORKDIR /app

FROM runtime AS final

COPY /docker/docker-entrypoint.sh /docker-entrypoint.sh
RUN ["chmod", "+x", "/docker-entrypoint.sh"]

WORKDIR /app
COPY /src/SbuBot/bin/Release/net5.0/ .

WORKDIR /
ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["dotnet", "SbuBot.dll"]