FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY /src /src
RUN dotnet restore /src/SbuBot/SbuBot.csproj
RUN dotnet build src/SbuBot/SbuBot.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish src/SbuBot/SbuBot.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SbuBot.dll"]
