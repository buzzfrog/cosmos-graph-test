SET DATABASE=dbtest
SET COLLECTION=coltest

ECHO %COSMOSDB_KEY%

SET CONNECTION_STRING="AccountEndpoint=https://%COSMOS_DB_NAME%.documents.azure.com:443/;AccountKey=%COSMOSDB_KEY%;ApiKind=Gremlin;database=%DATABASE%;collection=%COLLECTION%"

call bin\Release\graph-db-test.exe -b 500 -r 1 -c %CONNECTION_STRING% -n 5 -a 100 -w 0