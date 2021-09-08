cd C:\Users\Pawelb\Desktop\Programowanie\csharp\VolunteerComputing\CollatzExample

Write-Host 'Cleaning publish folder'

rm publish/*

Write-Host 'Publishing projects'

dotnet publish Collatz.Calculator/Collatz.Calculator.csproj -p:PublishProfile=Collatz.Calculator\Properties\PublishProfiles\FolderProfile.pubxml | out-null
dotnet publish Collatz.Merger/Collatz.Merger.csproj -p:PublishProfile=Collatz.Merger\Properties\PublishProfiles\FolderProfile.pubxml | out-null
dotnet publish Collatz.Generator/Collatz.Generator.csproj -p:PublishProfile=Collatz.Generator\Properties\PublishProfiles\FolderProfile.pubxml | out-null

$config = 'publish/config.json'

Write-Host 'Creating calculator CPU version'

Set-Content -Path $config -Value '{ "isCpu": true }'

Get-ChildItem -Path publish/Collatz.Calculator.exe, $config |
    Compress-Archive -DestinationPath .\publish\calculatorCpu.zip

Write-Host 'Creating calculator GPU version'

Set-Content -Path publish/config.json -Value '{ "isCpu": false }'

Get-ChildItem -Path publish/Collatz.Calculator.exe, $config |
    Compress-Archive -DestinationPath .\publish\calculatorGpu.zip