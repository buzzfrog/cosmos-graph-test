FROM microsoft/dotnet-framework:4.7.2-runtime-windowsservercore-ltsc2016 AS base
WORKDIR /app

FROM microsoft/dotnet-framework:4.7.2-sdk-windowsservercore-ltsc2016 AS build
WORKDIR /src
COPY graph-db-test.csproj graph-db-test/
RUN dotnet restore graph-db-test/graph-db-test.csproj
WORKDIR /src/graph-db-test
COPY . .
RUN dotnet build graph-db-test.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish graph-db-test.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["graph-db-test.exe"]