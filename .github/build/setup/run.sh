#!/bin/bash
echo "Starting application with http"
cd /app/appservice/
echo " "
echo " _______              _____ _____ _______ "
echo "|__   __|            |  __ \_   _|__   __|"
echo "   | | ___  _ __ ___ | |__) || |    | |   "
echo "   | |/ _ \| '_   _ \|  ___/ | |    | |   "
echo "   | | (_) | | | | | | |    _| |_   | |   "
echo "   |_|\___/|_| |_| |_|_|   |_____|  |_|   "
echo " "
echo " App is now running, wait few more seconds (2 min) for everything to run as it should...    "
dotnet TomPIT.Connected.dll
