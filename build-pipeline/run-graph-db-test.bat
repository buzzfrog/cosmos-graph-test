SET DATABASE=dbtest
SET COLLECTION=coltest

ECHO %COSMOS_DB_KEY%

SET CONNECTION_STRING="AccountEndpoint=https://%COSMOS_DB_NAME%.documents.azure.com:443/;AccountKey=%COSMOS_DB_KEY%;ApiKind=Gremlin;database=%DATABASE%;collection=%COLLECTION%"

:: This doesn't feel right way to get to the exe.
DIR %BUILD_SOURCESDIRECTORY%\src\graph-db-test\bin\Release\net461\*.*

