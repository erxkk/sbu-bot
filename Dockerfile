FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /kkommon
COPY ["kkommon/src/Kkommon/Kkommon.csproj", "src/Kkommon/"]
RUN dotnet build "Kkommon.csproj" -c Release
WORKDIR /src
COPY ["src/SbuBot/SbuBot.csproj", "SbuBot/"]
RUN dotnet restore "src/SbuBot/SbuBot.csproj"
COPY . .
WORKDIR "/src/SbuBot"
RUN dotnet build "SbuBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SbuBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SbuBot.dll"]
