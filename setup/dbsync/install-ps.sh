#!/bin/bash

install-ps () {
   if [ $1 == true ]; then
        echo "Installing wget, and unpacking powershell module"
        apt-get update && apt-get install wget -y
        wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        dpkg -i packages-microsoft-prod.deb
        apt-get update && apt-get install powershell
        echo "Powershell isntalled"
    else
        echo "Removing powershell from container"
        sudo apt-get remove powershell
    fi
}