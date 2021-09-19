cd C:\Users\Pawelb\Desktop\Programowanie\csharp\VolunteerComputing\PrimeExample

Write-Host 'Cleaning publish folder'

rm publish/*

Write-Host 'Publishing projects'

dotnet publish Prime.Calculator/Prime.Calculator.csproj -p:PublishProfile=Prime.Calculator\Properties\PublishProfiles\FolderProfile.pubxml | out-null
dotnet publish Prime.CalculatorGpu/Prime.CalculatorGpu.csproj -p:PublishProfile=Prime.CalculatorGpu\Properties\PublishProfiles\FolderProfile.pubxml | out-null
dotnet publish Prime.Merger/Prime.Merger.csproj -p:PublishProfile=Prime.Merger\Properties\PublishProfiles\FolderProfile.pubxml | out-null
dotnet publish ../CollatzExample/Collatz.Generator/Collatz.Generator.csproj -p:PublishProfile=Collatz.Generator\Properties\PublishProfiles\FolderProfile2.pubxml | out-null

dotnet publish Prime.Calculator/Prime.Calculator.csproj -p:PublishProfile=Prime.Calculator\Properties\PublishProfiles\FolderProfile1.pubxml | out-null
dotnet publish Prime.CalculatorGpu/Prime.CalculatorGpu.csproj -p:PublishProfile=Prime.CalculatorGpu\Properties\PublishProfiles\FolderProfile1.pubxml | out-null
dotnet publish Prime.Merger/Prime.Merger.csproj -p:PublishProfile=Prime.Merger\Properties\PublishProfiles\FolderProfile1.pubxml | out-null
dotnet publish ../CollatzExample/Collatz.Generator/Collatz.Generator.csproj -p:PublishProfile=Collatz.Generator\Properties\PublishProfiles\FolderProfile3.pubxml | out-null