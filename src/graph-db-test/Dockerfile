FROM microsoft/dotnet:2.1-runtime-alpine AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-alpine AS build
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
ENTRYPOINT ["dotnet", "graph-db-test.dll"]