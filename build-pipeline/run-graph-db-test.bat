SET DATABASE=dbtest
SET COLLECTION=coltest

ECHO %COSMOS_DB_KEY%

SET CONNECTION_STRING="AccountEndpoint=https://%COSMOS_DB_NAME%.documents.azure.com:443/;AccountKey=%COSMOS_DB_KEY%;ApiKind=Gremlin;database=%DATABASE%;collection=%COLLECTION%"

DIR *.*