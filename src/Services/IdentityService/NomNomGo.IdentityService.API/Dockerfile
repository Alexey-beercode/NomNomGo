FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5200

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj файлы для IdentityService
COPY ["IdentityService/NomNomGo.IdentityService.API/NomNomGo.IdentityService.API.csproj", "IdentityService/NomNomGo.IdentityService.API/"]
COPY ["IdentityService/NomNomGo.IdentityService.Infrastructure/NomNomGo.IdentityService.Infrastructure.csproj", "IdentityService/NomNomGo.IdentityService.Infrastructure/"]
COPY ["IdentityService/NomNomGo.IdentityService.Application/NomNomGo.IdentityService.Application.csproj", "IdentityService/NomNomGo.IdentityService.Application/"]
COPY ["IdentityService/NomNomGo.IdentityService.Domain/NomNomGo.IdentityService.Domain.csproj", "IdentityService/NomNomGo.IdentityService.Domain/"]

# Restore packages
RUN dotnet restore "IdentityService/NomNomGo.IdentityService.API/NomNomGo.IdentityService.API.csproj"

# Копируем весь исходный код
COPY . .

# Переходим в папку API проекта и собираем
WORKDIR "/src/IdentityService/NomNomGo.IdentityService.API"
RUN dotnet build "NomNomGo.IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NomNomGo.IdentityService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NomNomGo.IdentityService.API.dll"]