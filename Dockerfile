# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY UserManagement.Api.slnx ./
COPY Directory.Build.props ./
COPY UserManagement.Api/UserManagement.Api.csproj UserManagement.Api/
COPY UserManagement.Tests/UserManagement.Tests.csproj UserManagement.Tests/

RUN dotnet restore UserManagement.Api.slnx

COPY . .

RUN dotnet publish UserManagement.Api/UserManagement.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]
