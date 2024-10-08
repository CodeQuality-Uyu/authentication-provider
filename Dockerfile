#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CQ.AuthProvider.WebApi/CQ.AuthProvider.WebApi.csproj", "CQ.AuthProvider.WebApi/"]
COPY ["CQ.AuthProvider.BusinessLogic/CQ.AuthProvider.BusinessLogic.csproj", "CQ.AuthProvider.BusinessLogic/"]
COPY ["CQ.AuthProvider.DataAccess.EfCore/CQ.AuthProvider.DataAccess.EfCore.csproj","CQ.AuthProvider.DataAccess.EfCore"]
COPY ["CQ.IdentityProvider.EfCore/CQ.IdentityProvider.EfCore.csproj","CQ.IdentityProvider.EfCore"]
RUN dotnet restore "./CQ.AuthProvider.WebApi/CQ.AuthProvider.WebApi.csproj"
COPY . .
WORKDIR "/src/CQ.AuthProvider.WebApi"
RUN dotnet build "./CQ.AuthProvider.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CQ.AuthProvider.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CQ.AuthProvider.WebApi.dll"]