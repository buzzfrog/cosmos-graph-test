SET DATABASE=dbtest
SET COLLECTION=coltest

ECHO %COSMOS_DB_KEY%

SET CONNECTION_STRING="AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;ApiKind=Gremlin;database=%DATABASE%;collection=%COLLECTION%"

dotnet run -c Release -p %BUILD_SOURCESDIRECTORY%\src\graph-db-test\ -- -b 500 -r 1 -c %CONNECTION_STRING% -n 5 -a 100 -w 0