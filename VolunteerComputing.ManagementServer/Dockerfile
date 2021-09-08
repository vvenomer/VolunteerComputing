FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "VolunteerComputing.ManagementServer/Server/VolunteerComputing.ManagementServer.Server.csproj"
WORKDIR "/src/VolunteerComputing.ManagementServer/Server"
RUN dotnet build "VolunteerComputing.ManagementServer.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VolunteerComputing.ManagementServer.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolunteerComputing.ManagementServer.Server.dll"]