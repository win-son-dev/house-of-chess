# House of Chess

A real-time chess platform in the spirit of Lichess and chess.com, built as a portfolio project.

## Stack

- **Frontend:** React + Vite + TypeScript, [chessground](https://github.com/lichess-org/chessground), [chess.js](https://github.com/jhlywa/chess.js)
- **Backend:** ASP.NET Core (WebAPI) + SignalR
- **Realtime fan-out:** Redis backplane for SignalR (stateless, horizontally scalable)
- **Persistence:** PostgreSQL (everything — users, games, moves, ratings, analysis)
- **Auth:** Auth0 (JWT)
- **Anti-cheat:** Stockfish-driven accuracy/blunder analysis
- **Rating:** Elo
- **Local dev / prod:** Docker Compose on a DigitalOcean droplet, nginx in front of multiple API replicas

## Repository layout

```
house-of-chess/
├── server/HouseOfChess.Platform/        # .NET solution (Infrastructure / Services / Repositories / WebAPI / Packages / Tests)
├── web/                                  # React + Vite + TS
├── docker/                               # docker-compose, Dockerfiles, nginx.conf
└── .github/workflows/                    # CI
```

## Local development

```bash
# Build everything
cd server/HouseOfChess.Platform && dotnet build
cd ../../web && npm install && npm run dev

# Or use Docker (postgres, redis, stockfish, api×2, web, nginx LB)
cd docker && docker compose up --build
```

The compose setup runs **two API replicas behind nginx** to exercise the stateless SignalR + Redis backplane topology — that's the whole point.

## Architecture notes

- **Server-authoritative gameplay.** The client renders and proposes moves; the server validates against `HouseOfChess.Platform.Packages.ChessEngine` and updates clocks. Bad moves are rejected, never trusted from the client.
- **Stateless API.** Active-game state (current FEN, clocks, last-move timestamps, presence) lives in Redis. Any API replica can serve any game. No sticky sessions required.
- **Per-layer testing.** Services have xUnit + NSubstitute unit tests (mock repositories, drive every branch). Repositories use Testcontainers for real Postgres/Redis integration. Controllers/Hubs use `WebApplicationFactory` end-to-end.

## v1 scope

In: Auth0 login, quick-play matchmaking (bullet/blitz/rapid), real-time play, resign/draw/timeout/abort, spectating, game history + PGN export, post-game Stockfish analysis with accuracy scores and anti-cheat flagging, profile + Elo.

Out (post-MVP): puzzles, tournaments, variants, bot/API accounts, chat, friends, opening explorer, analysis board.
