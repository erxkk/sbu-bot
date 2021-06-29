FROM mcr.microsoft.com/dotnet/runtime:5.0

WORKDIR /app
COPY /src/SbuBot/bin/Release/net5.0/ .
ENTRYPOINT ["dotnet", "SbuBot.dll"]