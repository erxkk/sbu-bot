FROM mcr.microsoft.com/dotnet/runtime:5.0

WORKDIR /app
COPY /src/SbuBot/bin/Release/net5.0/ .

COPY /src/SbuBot/Migrations/ Migrations/

ENTRYPOINT ["dotnet", "SbuBot.dll"]
