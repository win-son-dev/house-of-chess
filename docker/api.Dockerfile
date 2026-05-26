# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY server/HouseOfChess.Platform/ ./
RUN dotnet publish HouseOfChess.Platform.WebAPI/HouseOfChess.Platform.WebAPI.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends stockfish curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HouseOfChess.Platform.WebAPI.dll"]
