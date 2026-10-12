# ============================================================
# ETAPA 1: BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Vercom.csproj", "./"]
RUN dotnet restore "Vercom.csproj"

COPY . .

RUN dotnet publish "Vercom.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ============================================================
# ETAPA 2: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# ✅ Usar el usuario 'app' que Microsoft ya incluye en la imagen (UID 1654)
# No necesitas crearlo, ya existe

COPY --from=build --chown=app:app /app/publish .

# ✅ Cambiar a usuario no-root (usando el que ya existe)
USER app

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "Vercom.dll"]