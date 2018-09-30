SET DATABASE=dbtest
SET COLLECTION=coltest

ECHO %COSMOS_DB_KEY%

SET CONNECTION_STRING="AccountEndpoint=https://%COSMOS_DB_NAME%.documents.azure.com:443/;AccountKey=%COSMOS_DB_KEY%;ApiKind=Gremlin;database=%DATABASE%;collection=%COLLECTION%"

:: This doesn't feel right way to get to the exe.
call %BUILD_SOURCESDIRECTORY%\src\graph-db-test\bin\Release\net461\graph-db-test.exe -b 500 -r 1 -c %CONNECTION_STRING% -n 5 -a 100 -w 0 

