#!/usr/bin/env bash
#
# House of Chess — compose stack verifier (BACKLOG 1.4.5).
# Cold-boots the full stack and proves that the two API replicas (api1, api2)
# both serve traffic via the nginx LB. Exits 0 on success, non-zero on failure.
#
# Usage: bash docker/verify.sh
set -euo pipefail

cd "$(dirname "$0")"

REQS="${REQS:-20}"
WAIT_SECONDS="${WAIT_SECONDS:-120}"
PROBE_URL="http://localhost:8080/api/games/ping"

echo "==> [1/5] Tearing down any prior stack (cold-boot test)…"
docker compose down -v --remove-orphans >/dev/null 2>&1 || true

echo "==> [2/5] Building + starting the stack…"
docker compose up --build -d

echo "==> [3/5] Waiting up to ${WAIT_SECONDS}s for the API to respond via nginx…"
deadline=$(( $(date +%s) + WAIT_SECONDS ))
until curl -fsS -o /dev/null "$PROBE_URL"; do
    if [ "$(date +%s)" -ge "$deadline" ]; then
        echo "FAIL: API did not respond within ${WAIT_SECONDS}s"
        docker compose ps
        docker compose logs --tail 50
        exit 1
    fi
    sleep 2
done

echo "==> [4/5] Hitting ${PROBE_URL} ${REQS} times via the LB…"
for _ in $(seq 1 "$REQS"); do
    curl -fsS -o /dev/null "$PROBE_URL"
done

echo "==> [5/5] Confirming both replicas served traffic (from nginx upstream log)…"
counts="$(docker compose logs web --since 5m 2>&1 \
    | grep -oE 'upstream="[^"]+"' \
    | sort | uniq -c || true)"

if [ -z "$counts" ]; then
    echo "FAIL: no upstream-tagged log lines found."
    docker compose logs web --tail 50
    exit 1
fi

echo "$counts"

distinct=$(printf '%s\n' "$counts" | awk 'NF' | wc -l | tr -d ' ')
if [ "$distinct" -lt 2 ]; then
    echo "FAIL: requests were served by ${distinct} upstream(s); expected api1 AND api2."
    exit 1
fi

echo
echo "OK — stack boots cold; both API replicas served traffic via nginx LB."
