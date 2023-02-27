
#!/bin/bash

originalDir=$1
newDir=$2
tmpDir="${originalDir}2"

echo $originalDir
echo $newDir
echo $tmpDir

mkdir -p "${originalDir}2"
git mv -k "${originalDir}/*" "${originalDir}2"
git mv -k "${originalDir}2/*" "$newDir"

rm $tmpDir
