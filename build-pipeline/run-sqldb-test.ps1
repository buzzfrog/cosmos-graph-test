$DATABASE = "master"
$USER = "sa"
$PASSWORD = "<YourStrong!Passw0rd>"
$CONTAINER = "sql-graph"
$PORT = 1433

try { docker rm -f $CONTAINER } catch {}
docker run -d -p ${PORT}:${PORT} --name $CONTAINER `
    -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$SA_PASSWORD" `
    -v ${pwd}\sql-server:c:\sql-server `
    microsoft/mssql-server-windows-developer

do {
    $DOCKER_STATUS = $(docker ps -f "name=$CONTAINER" --format "table {{.Status}}" | Select-Object -Last 1)
    Write-Host "SQL Server Status: $DOCKER_STATUS"
    Start-Sleep 1
} until ($DOCKER_STATUS -match "(healthy)")

docker exec -it $CONTAINER sqlcmd -S tcp:localhost,$PORT -d $DATABASE -i c:\sql-server\ddl.sql

$CONNECTION_STRING = "Server=tcp:localhost,$PORT;Initial Catalog=$DATABASE;Persist Security Info=False;User ID=$USER;Password=$PASSWORD;MultipleActiveResultSets=False;Connection Timeout=30;"

dotnet run -c Release -p .\src\graph-db-test\ --no-launch-profile -- -b 10000 -r 1 -c $CONNECTION_STRING -n 5 -a 10 -w 0