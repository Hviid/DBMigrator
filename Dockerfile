FROM mcr.microsoft.com/dotnet/runtime:5.0.9-alpine3.13-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.400-alpine3.13-amd64 AS build

WORKDIR /src
COPY ./DBMigrator.Console/DBMigrator.Console.csproj DBMigrator.Console/DBMigrator.Console.csproj
COPY ./DBMigrator/DBMigrator.csproj DBMigrator/DBMigrator.csproj
COPY ./DBMigrator.Test/DBMigrator.Test.csproj DBMigrator.Test/DBMigrator.Test.csproj
RUN dotnet restore -f DBMigrator.Console/DBMigrator.Console.csproj

COPY ../ .
RUN dotnet build -c Release -o /app DBMigrator.Console/DBMigrator.Console.csproj

FROM build AS test

RUN dotnet test

FROM base AS final

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "DBMigrator.Console.dll" ]
