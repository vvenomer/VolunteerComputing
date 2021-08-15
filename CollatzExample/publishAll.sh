#!/bin/bash

echo Clearing publish folder

rm publish/* > /dev/null 2>&1

echo Publishing projects

dotnet publish Collatz.Calculator/Collatz.Calculator.csproj -p:PublishProfile=./FolderProfile.pubxml > /dev/null 2>&1
dotnet publish Collatz.Merger/Collatz.Merger.csproj -p:PublishProfile=./FolderProfile.pubxml > /dev/null 2>&1
dotnet publish Collatz.Generator/Collatz.Generator.csproj -p:PublishProfile=./FolderProfile.pubxml > /dev/null 2>&1

CONFIG='publish/config.json'

echo Creating calculator CPU version

echo '{ "isCpu": true }' > $CONFIG

zip -j ./publish/calculatorCpu.zip publish/Collatz.Calculator $CONFIG

echo Creating calculator GPU version

echo '{ "isCpu": false }' > $CONFIG

zip -j ./publish/calculatorGpu.zip publish/Collatz.Calculator $CONFIG

