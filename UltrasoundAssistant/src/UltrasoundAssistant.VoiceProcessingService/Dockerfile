FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends \
    libstdc++6 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# каталог под модель
RUN mkdir -p /tmp/models && chmod -R 777 /tmp/models

USER $APP_UID

EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UltrasoundAssistant.VoiceProcessingService/UltrasoundAssistant.VoiceProcessingService.csproj", "src/UltrasoundAssistant.VoiceProcessingService/"]
RUN dotnet restore "./src/UltrasoundAssistant.VoiceProcessingService/UltrasoundAssistant.VoiceProcessingService.csproj"
COPY . .
WORKDIR "/src/src/UltrasoundAssistant.VoiceProcessingService"
RUN dotnet build "./UltrasoundAssistant.VoiceProcessingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UltrasoundAssistant.VoiceProcessingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UltrasoundAssistant.VoiceProcessingService.dll"]