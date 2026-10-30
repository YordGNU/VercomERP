# ============================================================
# VERCOM – Dockerfile multi-stage optimizado
# .NET 10 · Usuario no-root · Healthcheck con curl
# ============================================================

# ============================================================
# ETAPA 1: BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Cache de restore: solo copiamos el csproj primero
# Copiar y restaurar con cache persistente
COPY ["vercom.csproj", "./"]
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "vercom.csproj"

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "vercom.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false
# ============================================================
# ETAPA 2: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# ✅ TZ PRIMERO (bug #1 fix)
ENV TZ=America/Havana \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Herramientas: tzdata + curl (para healthcheck + debugging)
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      tzdata curl ca-certificates \
 && ln -snf /usr/share/zoneinfo/$TZ /etc/localtime \
 && echo $TZ > /etc/timezone \
 && rm -rf /var/lib/apt/lists/*

# Copiar artefactos con ownership del usuario 'app' (UID 1654, ya existe)
COPY --from=build --chown=app:app /app/publish .

# Cambiar a usuario no-root
USER app

EXPOSE 8080

# ✅ Healthcheck contra /health (bug #2 fix)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl --silent --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "vercom.dll"]