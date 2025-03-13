# Базовый образ для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 # Используем порт 80 для HTTP

# Базовый образ для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем и восстанавливаем зависимости
COPY ["WebApplication1.csproj", "."]
RUN dotnet restore

# Копируем весь проект и собираем его
COPY . .
RUN dotnet build -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Финальный образ для запуска
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]