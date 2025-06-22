FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5201

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj файлы для RecommendationService
COPY ["RecommendationService/RecommendationService.API/RecommendationService.API.csproj", "RecommendationService/RecommendationService.API/"]
COPY ["RecommendationService/RecommendationService.Application/RecommendationService.Application.csproj", "RecommendationService/RecommendationService.Application/"]
COPY ["RecommendationService/RecommendationService.Infrastructure/RecommendationService.Infrastructure.csproj", "RecommendationService/RecommendationService.Infrastructure/"]
COPY ["RecommendationService/RecommendationService.Domain/RecommendationService.Domain.csproj", "RecommendationService/RecommendationService.Domain/"]
COPY ["RecommendationService/RecommendationService.ML/RecommendationService.ML.csproj", "RecommendationService/RecommendationService.ML/"]

# Restore packages
RUN dotnet restore "RecommendationService/RecommendationService.API/RecommendationService.API.csproj"

# Копируем весь исходный код
COPY . .

# Переходим в папку API проекта
WORKDIR "/src/RecommendationService/RecommendationService.API"

# Build проекта
RUN dotnet build "RecommendationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RecommendationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecommendationService.API.dll"]