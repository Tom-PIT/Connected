#!/bin/bash

if [ $db_sync == true ]; then
    echo "Database sync in progress..."
    source /dbsync/install-ps.sh -true
    # pwsh -File "/dbdync/compare.ps1"
    # inset create.sql or update.json to db
    source /dbsync/install-ps.sh -false
else 
    echo "Skip database sync"
fi

echo "Starting Sys.dll as a base, other services will start if initialized only"
cd /app/sys/
dotnet Sys.dll --urls http://0.0.0.0:44001 &
while ! curl -s http://0.0.0.0:44001 > /dev/null; do sleep 1; done
echo -e "Sys.dll is ready" >> /connected.logs/sys.log

if [ $start_cdn == true ]; then
    cd /app/cdn
    dotnet TomPIT.Connected.Cdn.dll --urls http://0.0.0.0:44008 &
    while ! curl -s http://0.0.0.0:44008 > /dev/null; do sleep 1; done
    echo -e "Cdn.dll is ready" >> /connected.logs/cdn.log
fi

if [ $start_mng == true ]; then
    cd /app/mng
    dotnet TomPIT.Connected.Management.dll --urls http://0.0.0.0:44004 &
    while ! curl -s http://0.0.0.0:44004 > /dev/null; do sleep 1; done
    echo -e "Management.dll is ready" >> /connected.logs/mng.log
fi

if [ $start_app == true ]; then
    cd /app/app
    dotnet TomPIT.Connected.App.dll --urls http://0.0.0.0:44351 &
    while ! curl -s http://0.0.0.0:44351 > /dev/null; do sleep 1; done
    echo -e "App.dll is ready" >> /connected.logs/app.log
fi

if [ $start_dev == true ]; then
    cd /app/dev/
    dotnet TomPIT.Connected.Development.dll --urls http://0.0.0.0:44002 &
    while ! curl -s http://0.0.0.0:44002 > /dev/null; do sleep 1; done
    echo -e "Development.dll is ready" >> /connected.logs/dev.log
fi

if [ $start_search == true ]; then
    cd /app/search/
    dotnet TomPIT.Connected.Search.dll --urls http://0.0.0.0:51231 &
    while ! curl -s http://0.0.0.0:51231 > /dev/null; do sleep 1; done
    echo -e "Search.dll is ready" >> /connected.logs/search.log
fi

if [ $start_iot == true ]; then
    cd /app/iot/
    dotnet TomPIT.Connected.IoT.dll --urls http://0.0.0.0:44391 &
    while ! curl -s http://0.0.0.0:44391 > /dev/null; do sleep 1; done
    echo -e "IoT.dll is ready" >> /connected.logs/iot.log
fi

if [ $start_worker == true ]; then
    cd /app/worker/
    dotnet TomPIT.Connected.Worker.dll --urls http://0.0.0.0:44393 &
    while ! curl -s http://0.0.0.0:44393 > /dev/null; do sleep 1; done
    echo -e "Worker.dll is ready" >> /connected.logs/worker.log
fi

if [ $start_rest == true ]; then
    cd /app/rest/
    dotnet TomPIT.Connected.Rest.dll --urls http://0.0.0.0:44006 &
    while ! curl -s http://0.0.0.0:44006 > /dev/null; do sleep 1; done
    echo -e "Rest.dll is ready" >> /connected.logs/rest.log
fi

if [ $start_bigdata == true ]; then
    cd /app/bigdata/
    dotnet TomPIT.Connected.BigData.dll --urls http://0.0.0.0:5000 &
    while ! curl -s http://0.0.0.0:5000 > /dev/null; do sleep 1; done
    echo -e "BigData.dll is ready" >> /connected.logs/bigdata.log
fi

echo "Services started"
ps aux | grep dotnet >> /connected.logs/dotnet_processes.txt
# add send notification and complete log

wait