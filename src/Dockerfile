FROM microsoft/dotnet-framework:4.7.2-runtime-windowsservercore-ltsc2016 AS base
WORKDIR /app

FROM microsoft/dotnet-framework:4.7.2-sdk-windowsservercore-ltsc2016 AS build
WORKDIR /src
COPY cosmosdb-graph-test.csproj cosmosdb-graph-test/
RUN dotnet restore cosmosdb-graph-test/cosmosdb-graph-test.csproj
WORKDIR /src/cosmosdb-graph-test
COPY . .
RUN dotnet build cosmosdb-graph-test.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish cosmosdb-graph-test.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["cosmosdb-graph-test.exe"]