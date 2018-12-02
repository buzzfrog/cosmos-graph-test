#!/bin/bash

set -e
set -o pipefail
set +x

DATABASE="master"
SA_PASSWORD='<YourStrong!Passw0rd>'
CONTAINER="sql-graph"
PORT=1433

[ "$(docker ps -a | grep $CONTAINER)" ] && docker rm -f $CONTAINER
docker run -d -p $PORT:$PORT --name $CONTAINER \
    -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$SA_PASSWORD" \
    -v $(pwd)/sql-server:/sql-server \
    mcr.microsoft.com/mssql/server:2017-latest

while : ; do
    DOCKER_STATUS=$(docker ps -a -f "name=$CONTAINER" --format "table {{.Status}}" | grep "Up")
    echo "SQL Server Status: $DOCKER_STATUS"
    sleep 1

    [ -z "$DOCKER_STATUS" ] || break
done

docker exec $CONTAINER /opt/mssql-tools/bin/sqlcmd -S tcp:localhost,$PORT -d $DATABASE -i /sql-server/ddl.sql

$CONNECTION_STRING = "Server=tcp:localhost,$PORT;Initial Catalog=$DATABASE;Persist Security Info=False;User ID=sa;Password=$SA_PASSWORD;MultipleActiveResultSets=False;Connection Timeout=30;"

dotnet run -c Release -p ./src/graph-db-test --no-launch-profile -- -b 10000 -r 1 -c $CONNECTION_STRING -n 5 -a 10 -w 0