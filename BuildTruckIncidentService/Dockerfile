FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BuildTruckShared/BuildTruckShared.csproj", "BuildTruckShared/"]
COPY ["BuildTruckIncidentService/BuildTruckIncidentService.csproj", "BuildTruckIncidentService/"]
RUN dotnet restore "BuildTruckIncidentService/BuildTruckIncidentService.csproj"
COPY . .
WORKDIR "/src/BuildTruckIncidentService"
RUN dotnet build "BuildTruckIncidentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BuildTruckIncidentService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BuildTruckIncidentService.dll"]