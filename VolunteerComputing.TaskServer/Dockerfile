#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["VolunteerComputing.TaskServer/VolunteerComputing.TaskServer.csproj", "VolunteerComputing.TaskServer/"]
RUN dotnet restore "VolunteerComputing.TaskServer/VolunteerComputing.TaskServer.csproj"
COPY . .
WORKDIR "/src/VolunteerComputing.TaskServer"
RUN dotnet build "VolunteerComputing.TaskServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VolunteerComputing.TaskServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VolunteerComputing.TaskServer.dll"]