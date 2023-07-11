FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine3.18-amd64 AS base
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.18-amd64 AS build

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
