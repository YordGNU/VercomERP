#!/usr/bin/env bash
set -euo pipefail

IMAGEN="$1"
shift || true

INIT_DB=false
FORCE_DB=false
for arg in "$@"; do
    case $arg in
        --init-db)   INIT_DB=true ;;
        --force-db)  FORCE_DB=true ;;
    esac
done

cd /opt/vercom

GREEN='\033[0;32m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'
ok()   { echo -e "${GREEN}   OK $1${NC}"; }
info() { echo -e "   $1"; }
err()  { echo -e "${RED}   XX $1${NC}"; exit 1; }
hdr()  { echo -e "\n${CYAN}== $1 ==${NC}"; }

# ============================================================
# 1. PREPARACIÓN
# ============================================================
hdr "Preparación del entorno"

# 1a. Crear volúmenes si no existen
info "> Verificando volúmenes..."
for vol in vercom-mssql-data vercom_uploads-data vercom_grafana-data \
           vercom_loki-data vercom_prometheus-data vercom_nginx-logs; do
    if ! docker volume inspect "$vol" >/dev/null 2>&1; then
        docker volume create "$vol" >/dev/null
        info "  Creado: $vol"
    fi
done
ok "Volúmenes listos"

# 1b. Permisos del secreto JWT
info "> Verificando permisos del secreto JWT..."
if [ -f "secrets/jwt_key.txt" ]; then
    OWNER=$(stat -c '%u' secrets/jwt_key.txt)
    if [ "$OWNER" != "1654" ]; then
        sudo chown 1654:1654 secrets/jwt_key.txt 2>/dev/null || true
        info "  Permisos ajustados"
    fi
    ok "Secreto JWT OK"
else
    err "Falta secrets/jwt_key.txt"
fi

# 1c. Certificados SSL
info "> Verificando certificados SSL..."
if [ ! -f "nginx/certs/vercom.crt" ] || [ ! -f "nginx/certs/vercom.key" ]; then
    mkdir -p nginx/certs
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout nginx/certs/vercom.key \
        -out    nginx/certs/vercom.crt \
        -subj "/C=CU/ST=La Habana/L=Vedado/O=Demo S.U.R.L./CN=VERCOM-erp.lan" \
        -addext "subjectAltName=DNS:VERCOM-erp.lan,DNS:VERCOM-erp,DNS:localhost,IP:127.0.0.1" \
        2>/dev/null
    chmod 600 nginx/certs/vercom.key
    info "  Generados"
fi
ok "Certificados OK"

# ============================================================
# 2. CARGAR IMAGEN
# ============================================================
hdr "Carga de imagen"
info "> Cargando imagen $IMAGEN..."
docker load < "/tmp/$IMAGEN"
ok "Imagen cargada"

# ============================================================
# 3. LEVANTAR SQL SERVER
# ============================================================
hdr "SQL Server"
info "> Levantando SQL Server..."
docker compose -f docker-compose.yml up -d vercom-db
ok "SQL Server arrancando"

info "> Esperando healthy (max 120s)..."
WAITED=0
while [ $WAITED -lt 120 ]; do
    STATUS=$(docker inspect vercom-db --format='{{.State.Health.Status}}' 2>/dev/null || echo "unknown")
    if [ "$STATUS" = "healthy" ]; then
        ok "SQL Server healthy (${WAITED}s)"
        break
    fi
    sleep 5
    WAITED=$((WAITED + 5))
    echo -n "."
done
echo ""
if [ $WAITED -ge 120 ]; then
    err "SQL Server no llego a healthy"
fi

# ============================================================
# 4. INIT BD
# ============================================================
if [ "$INIT_DB" = true ]; then
    hdr "Inicialización de BD"
    INIT_ARGS=""
    [ "$FORCE_DB" = true ] && INIT_ARGS="--force"
    bash /opt/vercom/scripts/init-db.sh $INIT_ARGS
    ok "BD inicializada"
fi

# ============================================================
# 5. LEVANTAR STACK COMPLETO
# ============================================================
hdr "Stack completo"
info "> Levantando stack..."
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml up -d
ok "Stack arrancando"

info "> Esperando 30s..."
sleep 30

# nginx resuelve la IP del app al arrancar: al recrear el contenedor cambia y las
# peticiones seguirian yendo a la IP vieja (502). Un reinicio refresca el upstream.
info "> Reiniciando nginx para refrescar la IP del app..."
docker restart vercom-nginx >/dev/null
sleep 3

# ============================================================
# 6. ESTADO
# ============================================================
hdr "Estado del stack"
docker compose -f docker-compose.yml -f docker-compose.prod.yml -f docker-compose.monitoring.yml ps

# ============================================================
# 7. LIMPIEZA
# ============================================================
hdr "Limpieza"
rm -f "/tmp/$IMAGEN"
docker image prune -f >/dev/null 2>&1 || true
ok "Limpieza completada"

echo ""
echo -e "${GREEN}=================================================${NC}"
echo -e "${GREEN} Deploy completado${NC}"
echo -e "${GREEN}=================================================${NC}"