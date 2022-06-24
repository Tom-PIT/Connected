
#!/bin/bash

echo $1
echo $2

originalDir=$1
newDir=$2

mkdir ${originalDir}2
git mv ${originalDir}/* ${originalDir}2
git mv ${originalDir}2/* $newDir
