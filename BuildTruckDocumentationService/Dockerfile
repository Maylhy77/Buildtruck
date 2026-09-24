FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BuildTruckShared/BuildTruckShared.csproj", "BuildTruckShared/"]
COPY ["BuildTruckDocumentationService/BuildTruckDocumentationService.csproj", "BuildTruckDocumentationService/"]
RUN dotnet restore "BuildTruckDocumentationService/BuildTruckDocumentationService.csproj"
COPY . .
WORKDIR "/src/BuildTruckDocumentationService"
RUN dotnet build "BuildTruckDocumentationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BuildTruckDocumentationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BuildTruckDocumentationService.dll"]
