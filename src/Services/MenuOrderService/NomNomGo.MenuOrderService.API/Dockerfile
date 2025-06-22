FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5202

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj файлы для MenuOrderService
COPY ["MenuOrderService/NomNomGo.MenuOrderService.API/NomNomGo.MenuOrderService.API.csproj", "MenuOrderService/NomNomGo.MenuOrderService.API/"]
COPY ["MenuOrderService/NomNomGo.MenuOrderService.Application/NomNomGo.MenuOrderService.Application.csproj", "MenuOrderService/NomNomGo.MenuOrderService.Application/"]
COPY ["MenuOrderService/NomNomGo.MenuOrderService.Infrastructure/NomNomGo.MenuOrderService.Infrastructure.csproj", "MenuOrderService/NomNomGo.MenuOrderService.Infrastructure/"]
COPY ["MenuOrderService/NomNomGo.MenuOrderService.Domain/NomNomGo.MenuOrderService.Domain.csproj", "MenuOrderService/NomNomGo.MenuOrderService.Domain/"]

# Копируем csproj файлы для RecommendationService (зависимости)
COPY ["RecommendationService/RecommendationService.Application/RecommendationService.Application.csproj", "RecommendationService/RecommendationService.Application/"]
COPY ["RecommendationService/RecommendationService.Infrastructure/RecommendationService.Infrastructure.csproj", "RecommendationService/RecommendationService.Infrastructure/"]
COPY ["RecommendationService/RecommendationService.Domain/RecommendationService.Domain.csproj", "RecommendationService/RecommendationService.Domain/"]
COPY ["RecommendationService/RecommendationService.ML/RecommendationService.ML.csproj", "RecommendationService/RecommendationService.ML/"]

# Restore packages
RUN dotnet restore "MenuOrderService/NomNomGo.MenuOrderService.API/NomNomGo.MenuOrderService.API.csproj"

# Копируем весь исходный код
COPY . .

# Переходим в папку API проекта
WORKDIR "/src/MenuOrderService/NomNomGo.MenuOrderService.API"

# Build проекта
RUN dotnet build "NomNomGo.MenuOrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NomNomGo.MenuOrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NomNomGo.MenuOrderService.API.dll"]