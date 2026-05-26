# House of Chess — Project Backlog

> **GitHub Issues are the canonical, real-time backlog.** This file is a static mirror for at-a-glance context. Pick up the next [open `type:task` issue](https://github.com/win-son-dev/house-of-chess/issues?q=is%3Aissue+is%3Aopen+label%3A%22type%3Atask%22) — Tasks are smaller-than-story sub-issues created under a Story when work begins.
>
> - Feature issues: [#1–#16](https://github.com/win-son-dev/house-of-chess/issues?q=is%3Aissue+label%3A%22type%3Afeature%22) (one per F1–F16, in the same order)
> - Story issues: [#17–#78](https://github.com/win-son-dev/house-of-chess/issues?q=is%3Aissue+label%3A%22type%3Astory%22) (BACKLOG order)
> - Task issues: sub-issues of Stories, created just-in-time

Hierarchy: **Feature → Story → Task**. A story is done when every task under it is ticked **and** has the test coverage the team agreed on (services = mocked unit tests, repos = Testcontainers integration tests, controllers/hubs = WebApplicationFactory end-to-end).

Status: ✅ done · 🔄 partial · ⏳ pending

> **Last audited:** 2026-05-26 against actual repo state. See "Audit notes" at the bottom for what was reclassified since the last version.

---

## F1 ([#1](https://github.com/win-son-dev/house-of-chess/issues/1)) — Foundation & Infrastructure 🔄

The monorepo, layered .NET solution, React app, Docker compose, CI. Everything else builds on this.

### Story 1.1 ([#17](https://github.com/win-son-dev/house-of-chess/issues/17)) — Repo & .NET solution scaffold ✅
- [x] 1.1.1 (BE) Create `HouseOfChess.Platform.sln` + 6 projects (Infrastructure / Services / Repositories / WebAPI / Packages.ChessEngine / Tests)
- [x] 1.1.2 (BE) Wire project references per layering rules
- [x] 1.1.3 (BE) Install NuGet packages (EF Core, Npgsql, SignalR Redis, JWT Bearer, Swashbuckle, HealthChecks, NSubstitute, Testcontainers, Chess)

### Story 1.2 ([#18](https://github.com/win-son-dev/house-of-chess/issues/18)) — React + Vite + TS web app ✅
- [x] 1.2.1 (FE) Scaffold via `create-vite` `react-ts`
- [x] 1.2.2 (FE) Install Tailwind v4 via `@tailwindcss/vite`
- [x] 1.2.3 (FE) Install shadcn/ui (manual setup, pinned `shadcn@4.0.8`)
- [x] 1.2.4 (FE) Add `chess.js`, `chessground`, `@microsoft/signalr`, `react-router-dom`, `@auth0/auth0-react`

### Story 1.3 ([#19](https://github.com/win-son-dev/house-of-chess/issues/19)) — Layered DI, options, constants ✅
- [x] 1.3.1 (BE) `Infrastructure/Constants/ConfigSections.cs`
- [x] 1.3.2 (BE) Typed options classes (Auth0, Elo, Stockfish, ConnectionStrings, Cors)
- [x] 1.3.3 (BE) `AddOptions<T>().Bind().Validate().ValidateOnStart()` for fail-fast misconfig
- [x] 1.3.4 (BE) Defer JWT / Redis backplane / CORS / DbContext via `IConfigureOptions` + factory delegates

### Story 1.4 ([#20](https://github.com/win-son-dev/house-of-chess/issues/20)) — Docker setup 🔄
- [x] 1.4.1 (Ops) `api.Dockerfile` multi-stage + `apt-get install stockfish`
- [x] 1.4.2 (Ops) `web.Dockerfile` multi-stage (Node build → nginx static)
- [x] 1.4.3 (Ops) `nginx.conf` LB with WebSocket upgrade for `/hubs/*`
- [x] 1.4.4 (Ops) `docker-compose.yml` with postgres + redis + api1 + api2 + web + healthchecks
- [ ] 1.4.5 (Ops) **Verify** `docker compose up --build` boots end-to-end on a clean machine, both API replicas reachable, web → api1/api2 via nginx LB

### Story 1.5 ([#21](https://github.com/win-son-dev/house-of-chess/issues/21)) — CI workflow 🔄
- [x] 1.5.1 (Ops) `.github/workflows/ci.yml` parallel server + web jobs
- [ ] 1.5.2 (Ops) Push branch and confirm CI green on GitHub

---

## F2 ([#2](https://github.com/win-son-dev/house-of-chess/issues/2)) — Identity & Onboarding 🔄

Users sign in via Auth0. On first login the SPA calls `POST /api/account/onboarding`, which reads `sub` from the JWT, creates a local `User` row, and returns our internal `userId` (Guid). Every subsequent server-side call looks up the user by `auth0_sub` (indexed). **No Auth0 Action, no M2M / Management API** — that path was dropped 2026-05-24 in favor of the cheaper DB lookup.

### Story 2.1 ([#22](https://github.com/win-son-dev/house-of-chess/issues/22)) — Auth0 SPA client integration ✅
- [x] 2.1.1 (FE) Wrap with `Auth0Provider` + `BrowserRouter` + `useNavigate` redirect callback
- [x] 2.1.2 (FE) `RequireAuth` wrapper around protected routes
- [x] 2.1.3 (FE) Tenant credentials in `.env`; `vite --port 5173 --strictPort`
- [x] 2.1.4 (Ops) Allowed Callback / Logout / Web Origin URLs configured in Auth0 dashboard
- [x] 2.1.5 — Universal Login round-trip verified

### Story 2.2 ([#23](https://github.com/win-son-dev/house-of-chess/issues/23)) — API resource & access tokens ✅
- [x] 2.2.1 (Ops) API registered in Auth0 (identifier `https://api.houseofchess.local`)
- [x] 2.2.2 (FE) `VITE_AUTH0_AUDIENCE` set; `getAccessTokenSilently` returns an audience-scoped JWT
- [x] 2.2.3 (BE) `Auth0.Audience` bound + validated in `appsettings.json`
- [x] 2.2.4 (FE) JWT attached to `fetch` (`api.ts`) and SignalR (`accessTokenFactory` in `signalr.ts`)
- [ ] 2.2.5 (BE+FE) Smoke-test: `GET /api/games/ping` 200 with token, 401 without — add to a manual test checklist or an integration test

### Story 2.3 ([#24](https://github.com/win-son-dev/house-of-chess/issues/24)) — Onboarding endpoint (sub → internal userId) ✅
- [x] 2.3.1 (BE) `IUserRepository` + `UserRepository` with `GetByAuth0SubAsync` / `CreateAsync`
- [x] 2.3.2 (BE) `POST /api/account/onboarding` returns `(userId, username, isNew)`
- [x] 2.3.3 (BE) `ClaimsPrincipalExtensions.GetAuth0Sub()` helper
- [x] 2.3.4 (BE) Reject duplicate usernames — `AccountService` pre-checks via `UsernameExistsAsync`, controller returns 409 Conflict
- [x] 2.3.5 (BE) Validate username format server-side (length + pattern) via `UsernameOptions`; controller returns 400 BadRequest
- [x] 2.3.6 (BE) Race-condition guard — `UserRepository.CreateAsync` catches Postgres `23505` on `IX_Users_Username` and returns null; service translates to 409

### Story 2.4 ([#25](https://github.com/win-son-dev/house-of-chess/issues/25)) — First-login onboarding UX ⏳
- [ ] 2.4.1 (FE) Onboarding page at `/onboarding` with username input
- [ ] 2.4.2 (FE) Inline validation + "already taken" error mapped from 409
- [ ] 2.4.3 (FE) On bootstrap, call `GET /api/account/me` (see 2.5) — if it returns 404 / "no user row", redirect to `/onboarding`; otherwise hydrate `MeProvider`
- [ ] 2.4.4 (FE) After successful onboarding, navigate to `/lobby` and refresh `MeProvider`

### Story 2.5 ([#26](https://github.com/win-son-dev/house-of-chess/issues/26)) — "Who am I" endpoint ⏳
- [ ] 2.5.1 (BE) `GET /api/account/me` → `{ userId, username, ratings }`; 404 if user row doesn't exist yet
- [ ] 2.5.2 (FE) `MeProvider` calls it on mount, holds the internal userId for the rest of the app

### Story 2.6 ([#27](https://github.com/win-son-dev/house-of-chess/issues/27)) — Tests ⏳
- [ ] 2.6.1 (BE) `AccountController` integration test (WebApplicationFactory + Testcontainers Postgres) — happy path, duplicate username 409, unauthenticated 401
- [ ] 2.6.2 (BE) `UserRepository` integration test — unique-constraint behavior on Auth0Sub and Username
- [ ] 2.6.3 (FE) Onboarding form validation + 409-error rendering

---

## F3 ([#3](https://github.com/win-son-dev/house-of-chess/issues/3)) — Chess Engine ✅

**Decision (audited 2026-05-26):** Server-side legal-move generation, FEN/SAN handling, and game-over detection are delegated to the `Chess` NuGet package, wrapped by `Packages.ChessEngine/MoveEngine.cs`. We do **not** write our own bitboards. The remaining work is wrapper coverage and PGN export.

### Story 3.1 ([#28](https://github.com/win-son-dev/house-of-chess/issues/28)) — MoveEngine wrapper ✅
- [x] 3.1.1 (BE) `MoveEngine.Apply(fen, uci)` returns `MoveOutcome` (Accepted, RejectionReason, NewFen, San, FinalResult)
- [x] 3.1.2 (BE) `EngineGameResult` enum (WhiteWin / BlackWin / Draw)
- [x] 3.1.3 (BE) Auto-endgame rules (checkmate, stalemate, insufficient material, 50-move, 3-fold) enabled via `AutoEndgameRules.All`

### Story 3.2 ([#29](https://github.com/win-son-dev/house-of-chess/issues/29)) — PGN export ⏳
- [ ] 3.2.1 (BE) `IPgnExportService` in Infrastructure; `PgnExportService` in Services that builds PGN from a `Game` + its ordered `GameMove` list (tags: White/Black/Result/Date/TimeControl)
- [ ] 3.2.2 (BE) On game finish, populate `Game.Pgn` (currently never written)
- [ ] 3.2.3 (BE) Unit tests against a known scholar's-mate / fool's-mate sequence

### Story 3.3 ([#30](https://github.com/win-son-dev/house-of-chess/issues/30)) — Wrapper tests ⏳
- [ ] 3.3.1 (BE) Starting position: legal move count = 20, no result
- [ ] 3.3.2 (BE) Scholar's mate (1.e4 e5 2.Bc4 Nc6 3.Qh5 Nf6?? 4.Qxf7#) → `EngineGameResult.WhiteWin`
- [ ] 3.3.3 (BE) Fool's mate (1.f3 e5 2.g4 Qh4#) → `EngineGameResult.BlackWin`
- [ ] 3.3.4 (BE) Illegal move (`e2e5`) → `Accepted=false`, RejectionReason populated
- [ ] 3.3.5 (BE) Threefold repetition reached → Draw
- [ ] 3.3.6 (BE) Promotion UCI (`e7e8q`) accepted, SAN includes `=Q`

---

## F4 ([#4](https://github.com/win-son-dev/house-of-chess/issues/4)) — Live Gameplay (real-time) 🔄

The core loop: two players → server validates each move → broadcasts to both via SignalR → server updates clocks → ends game on terminal condition.

### Story 4.1 ([#31](https://github.com/win-son-dev/house-of-chess/issues/31)) — `GameService` orchestration 🔄
- [x] 4.1.1 (BE) `SubmitMoveAsync` — turn check, FEN fetch (Redis), validate via `MoveEngine`, append move (Postgres), update FEN (Redis), `Finish` on terminal result
- [ ] 4.1.2 (BE) **Server-side clocks** — load `TimeControl` (e.g. `5+0`) → maintain `(whiteMs, blackMs, lastMoveAt)` in Redis via `IGameStateRepository.SetClocksAsync`; deduct elapsed since previous move on each `SubmitMoveAsync`; reject move with `MoveResult.Accepted=false` if mover's clock <= 0
- [ ] 4.1.3 (BE) Atomic FEN+clock swap via a Lua script on the Redis repo (avoid lost-update under concurrent broadcasts)
- [ ] 4.1.4 (BE) On terminal result: apply Elo (see F9.2), persist ratings, enqueue analysis (see F10.4)

### Story 4.2 ([#32](https://github.com/win-son-dev/house-of-chess/issues/32)) — SignalR wire format 🔄
- [x] 4.2.1 (BE) `JoinGame`, `LeaveGame`, `SubmitMove`, `EnqueueMatch`, `CancelMatch` hub methods
- [x] 4.2.2 (BE) `moveApplied` broadcast on accepted move
- [x] 4.2.3 (BE) `matchFound` broadcast on pair
- [ ] 4.2.4 (BE+FE) `clockUpdate` broadcast every move (and from the tick service — see F6.3)
- [ ] 4.2.5 (BE+FE) `gameEnded` broadcast with `{ result, reason, ratingDelta }`
- [ ] 4.2.6 (BE+FE) `moveRejected` to the calling client only with `RejectionReason`

### Story 4.3 ([#33](https://github.com/win-son-dev/house-of-chess/issues/33)) — Client integration 🔄
- [x] 4.3.1 (FE) `chessground` mounted; legal-dest computation from `chess.js`
- [x] 4.3.2 (FE) `HubConnection` with `accessTokenFactory`
- [x] 4.3.3 (FE) `JoinGame` on mount, `LeaveGame` on unmount
- [x] 4.3.4 (FE) Handle `moveApplied` (board re-render)
- [ ] 4.3.5 (FE) Handle `clockUpdate` (clock display)
- [ ] 4.3.6 (FE) Handle `gameEnded` (modal with result + Elo delta + "back to lobby")
- [ ] 4.3.7 (FE) Handle `moveRejected` (snap piece back, toast)
- [ ] 4.3.8 (FE) Optimistic ghost piece on user move; revert on `moveRejected`

### Story 4.4 ([#34](https://github.com/win-son-dev/house-of-chess/issues/34)) — Stateless replicas verification (portfolio showcase) ⏳
- [ ] 4.4.1 (Ops) `docker compose up` with `api1` + `api2`
- [ ] 4.4.2 (Ops) Open game in two browsers; pin each to a different replica (separate ports or devtools network override)
- [ ] 4.4.3 (Ops) Confirm both clients see the same move when one player moves — proves Redis backplane works
- [ ] 4.4.4 (Docs) Capture a short screen recording / GIF for the README

### Story 4.5 ([#35](https://github.com/win-son-dev/house-of-chess/issues/35)) — Tests 🔄
- [x] 4.5.1 (BE) `GameServiceTests` skeleton in place (happy-path covered)
- [ ] 4.5.2 (BE) Add: illegal-move, wrong-turn, move-after-finish, non-player tries to move
- [ ] 4.5.3 (BE) End-to-end: `WebApplicationFactory` + Testcontainers Postgres + Redis, JWT-authenticated SignalR client plays a move and observes `moveApplied`
- [ ] 4.5.4 (FE) Component test for `Game` page — mock hub, assert UI updates on each broadcast type

---

## F5 ([#5](https://github.com/win-son-dev/house-of-chess/issues/5)) — Matchmaking 🔄

Player picks a time control → server pairs them with an opponent in the same rating band. Redis-backed so any API replica can pair.

### Story 5.1 ([#36](https://github.com/win-son-dev/house-of-chess/issues/36)) — Queue model 🔄
- [x] 5.1.1 (BE) `IMatchmakingQueueRepository` (Enqueue / TryDequeue / Remove), Redis-backed
- [x] 5.1.2 (BE) `MatchmakingService.EnqueueAsync` — naive pairing (first opponent in queue), random color assignment
- [ ] 5.1.3 (BE) Rating-bucket pairing — Redis `ZSET matchmaking:{tc}:{bucket}` keyed by userId, score `joined_ts_ms`; try ±100 rating, expand ±50 every 5s up to ±400
- [ ] 5.1.4 (BE) On pair: write initial Redis state (starting FEN, clocks at full) before broadcasting

### Story 5.2 ([#37](https://github.com/win-son-dev/house-of-chess/issues/37)) — Pairing notification ✅
- [x] 5.2.1 (BE) Hub broadcasts `matchFound` to both users via per-user group
- [x] 5.2.2 (FE) Lobby listens for `matchFound`; navigates to `/game/:id`

### Story 5.3 ([#38](https://github.com/win-son-dev/house-of-chess/issues/38)) — Cancellation ✅
- [x] 5.3.1 (BE) `MatchmakingService.CancelAsync`
- [x] 5.3.2 (FE) Cancel button while waiting

### Story 5.4 ([#39](https://github.com/win-son-dev/house-of-chess/issues/39)) — Tests ⏳
- [ ] 5.4.1 (BE) `MatchmakingServiceTests` — mock queue repo, verify expanding-bucket logic (after 5.1.3 lands)
- [ ] 5.4.2 (BE) Integration test against Testcontainers Redis — two users enqueue → both paired

---

## F6 ([#6](https://github.com/win-son-dev/house-of-chess/issues/6)) — Game Lifecycle (Resign / Draw / Timeout / Abort) ⏳

A game can end for reasons beyond checkmate. Each path needs server logic + UI controls. `GameService.ResignAsync` / `OfferDrawAsync` exist as `NotImplementedException` stubs today.

### Story 6.1 ([#40](https://github.com/win-son-dev/house-of-chess/issues/40)) — Resign ⏳
- [ ] 6.1.1 (BE) Implement `GameService.ResignAsync(gameId, userId)` — verify user is a player, mark game finished with opponent as winner, persist
- [ ] 6.1.2 (BE) `GameHub.Resign(gameId)` method
- [ ] 6.1.3 (FE) Resign button in `Game` page; confirmation prompt

### Story 6.2 ([#41](https://github.com/win-son-dev/house-of-chess/issues/41)) — Draw offer ⏳
- [ ] 6.2.1 (BE) State machine in `GameService`: `OfferDrawAsync` / `AcceptDrawAsync` / `DeclineDrawAsync`; offer is single-slot, expires on next move from the offerer's opponent
- [ ] 6.2.2 (BE) `GameHub.OfferDraw` / `AcceptDraw` / `DeclineDraw`
- [ ] 6.2.3 (FE) Offer button + opponent-side accept/decline banner

### Story 6.3 ([#42](https://github.com/win-son-dev/house-of-chess/issues/42)) — Timeout / clock tick service ⏳
- [ ] 6.3.1 (BE) `ClockTickService : IHostedService` — every 250ms, iterate active games' Redis clocks; for any whose mover hit 0, call `GameService.FlagAsync(gameId)` (new method) and broadcast `gameEnded`
- [ ] 6.3.2 (BE) FIDE edge: if opponent has insufficient material when the flag falls → draw, not loss
- [ ] 6.3.3 (BE) `clockUpdate` broadcast frequency: every move (not every tick) — keep network traffic low

### Story 6.4 ([#43](https://github.com/win-son-dev/house-of-chess/issues/43)) — Abort ⏳
- [ ] 6.4.1 (BE) Allowed while `MoveCount < 2` with no Elo change; uses the same `Finish` path with `Result = "*"` and a `Reason = Aborted` column
- [ ] 6.4.2 (FE) Abort button shows in the first 2 moves only

### Story 6.5 ([#44](https://github.com/win-son-dev/house-of-chess/issues/44)) — Tests ⏳
- [ ] 6.5.1 (BE) `GameServiceTests` — one per termination path (resign, draw-accepted, draw-declined, timeout, abort)
- [ ] 6.5.2 (BE) `ClockTickServiceTests` — fake clock + in-memory state, verify a game flagged at the right tick

---

## F7 ([#7](https://github.com/win-son-dev/house-of-chess/issues/7)) — Spectator Mode ⏳

Read-only viewing of live games. (Chat is out of v1.)

### Story 7.1 ([#45](https://github.com/win-son-dev/house-of-chess/issues/45)) — Live games feed ⏳
- [ ] 7.1.1 (BE) `GET /api/games/live` — in-progress games with players + ratings + current time control
- [ ] 7.1.2 (FE) Lobby tab showing live games; click → spectate

### Story 7.2 ([#46](https://github.com/win-son-dev/house-of-chess/issues/46)) — Spectator hub join ⏳
- [ ] 7.2.1 (BE) `GameHub.SpectateGame(gameId)` — joins group as read-only; mark connection in `Context.Items` so `SubmitMove`/`Resign` return 403
- [ ] 7.2.2 (FE) `Game` page detects spectator mode (no player match) and disables input

### Story 7.3 ([#47](https://github.com/win-son-dev/house-of-chess/issues/47)) — Tests ⏳
- [ ] 7.3.1 (BE) Hub integration test: spectator receives `moveApplied`; spectator attempting `SubmitMove` is rejected

---

## F8 ([#8](https://github.com/win-son-dev/house-of-chess/issues/8)) — Game History & PGN Export ⏳

Profile/history page lists finished games; click → replay; download PGN. Depends on F3.2 (PGN export) and F11 (profile page).

### Story 8.1 ([#48](https://github.com/win-son-dev/house-of-chess/issues/48)) — History list ⏳
- [ ] 8.1.1 (BE) `GET /api/users/{id}/games?page=&size=` — paginated finished games for a user
- [ ] 8.1.2 (FE) Profile page "Games" tab

### Story 8.2 ([#49](https://github.com/win-son-dev/house-of-chess/issues/49)) — Single-game replay ⏳
- [ ] 8.2.1 (BE) `GET /api/games/{id}` already returns a snapshot — extend to include full move list + result reason
- [ ] 8.2.2 (FE) Read-only chessground with move navigation (← → Home End)

### Story 8.3 ([#50](https://github.com/win-son-dev/house-of-chess/issues/50)) — PGN download ⏳
- [ ] 8.3.1 (BE) `GET /api/games/{id}/pgn` → `Content-Type: application/x-chess-pgn` attachment, body from `Game.Pgn` (populated by F3.2.2)

---

## F9 ([#9](https://github.com/win-son-dev/house-of-chess/issues/9)) — Rating System (Elo) 🔄

Each completed game adjusts both players' ratings per Elo. K-factor in config.

### Story 9.1 ([#51](https://github.com/win-son-dev/house-of-chess/issues/51)) — Core math ✅
- [x] 9.1.1 (BE) `EloRatingService.Compute` with K from `EloOptions`
- [x] 9.1.2 (BE) Unit tests (7 cases incl. K-factor scaling + points conservation)

### Story 9.2 ([#52](https://github.com/win-son-dev/house-of-chess/issues/52)) — Apply on game end ⏳
- [ ] 9.2.1 (BE) `GameService.FinishAsync` (or termination paths in F6) calls `IRatingService.Compute`; updates `User.EloBullet/Blitz/Rapid` for the right bucket based on `Game.TimeControl`
- [ ] 9.2.2 (BE) New `RatingHistory` entity + migration (UserId, GameId, Bucket, RatingBefore, RatingAfter, Delta, At)
- [ ] 9.2.3 (BE) Persist a `RatingHistory` row per player on every finish
- [ ] 9.2.4 (BE+FE) Surface Elo delta in the `gameEnded` broadcast payload (see F4.2.5)

### Story 9.3 ([#53](https://github.com/win-son-dev/house-of-chess/issues/53)) — Provisional / variable K (v1.5 stretch) ⏳
- [ ] 9.3.1 (BE) Track `GamesPlayed` per bucket on `User`; K=40 until 30 games, then K=20
- [ ] 9.3.2 (BE) Update `EloRatingServiceTests` to assert variable-K behavior

---

## F10 ([#10](https://github.com/win-son-dev/house-of-chess/issues/10)) — Anti-Cheat & Stockfish Analysis ⏳

Post-game Stockfish eval → accuracy %, blunder/mistake/inaccuracy classification, anti-cheat flag when player's centipawn loss is suspiciously low.

### Story 10.1 ([#54](https://github.com/win-son-dev/house-of-chess/issues/54)) — `StockfishService` UCI wrapper ⏳
- [ ] 10.1.1 (BE) Spawn process, communicate via stdin/stdout
- [ ] 10.1.2 (BE) Pool of long-lived processes (don't spawn per call); pool size from `StockfishOptions`
- [ ] 10.1.3 (BE) Parse `info ... cp <N>` and `bestmove <uci>` responses
- [ ] 10.1.4 (BE) Integration test gated by env (skip if no stockfish binary)

### Story 10.2 ([#55](https://github.com/win-son-dev/house-of-chess/issues/55)) — Post-game analysis pipeline ⏳
- [ ] 10.2.1 (BE) `IAnalysisService.AnalyzeFinishedGameAsync` — evaluate each position, compute centipawn delta per move, classify (best / good / inaccuracy / mistake / blunder)
- [ ] 10.2.2 (BE) Accuracy % per player (Lichess formula or simple weighted avg)
- [ ] 10.2.3 (BE) New `AnalysisReport` entity + migration; persist per-game

### Story 10.3 ([#56](https://github.com/win-son-dev/house-of-chess/issues/56)) — Anti-cheat heuristic ⏳
- [ ] 10.3.1 (BE) `AntiCheatService` — avg centipawn loss + engine-match rate
- [ ] 10.3.2 (BE) Flag if both metrics cross thresholds from config; persist `AntiCheatFlag` row
- [ ] 10.3.3 (BE) No auto-bans v1 — manual review only

### Story 10.4 ([#57](https://github.com/win-son-dev/house-of-chess/issues/57)) — Background runner ⏳
- [ ] 10.4.1 (BE) `AnalysisQueueService : IHostedService` (or MassTransit consumer) that reads finished-game IDs off a Redis list and runs analysis
- [ ] 10.4.2 (BE) `GameService` enqueues the task on terminal result
- [ ] 10.4.3 (FE) When analysis lands, show accuracy + classification icons in the replay view (poll `GET /api/games/{id}` until `analysisReady`)

### Story 10.5 ([#58](https://github.com/win-son-dev/house-of-chess/issues/58)) — Tests ⏳
- [ ] 10.5.1 (BE) `AnalysisServiceTests` — mock `StockfishService`, verify classification math at boundaries
- [ ] 10.5.2 (BE) `AntiCheatServiceTests` at threshold boundaries

---

## F11 ([#11](https://github.com/win-son-dev/house-of-chess/issues/11)) — Profile Page ⏳

Logged-in user (and others) can see ratings + history + (only for self/admin) anti-cheat status.

### Story 11.1 ([#59](https://github.com/win-son-dev/house-of-chess/issues/59)) — Profile endpoint ⏳
- [ ] 11.1.1 (BE) `GET /api/users/{id}/profile` — username, per-bucket ratings, total games, recent results
- [ ] 11.1.2 (FE) Profile page at `/profile/:id` (and shortcut `/profile` → self)

### Story 11.2 ([#60](https://github.com/win-son-dev/house-of-chess/issues/60)) — Self vs other visibility ⏳
- [ ] 11.2.1 (BE) Hide anti-cheat flag from public profiles; expose only to the user themselves (and admins, if/when added)

---

## F12 ([#12](https://github.com/win-son-dev/house-of-chess/issues/12)) — Database Migrations & Real Schema 🔄

### Story 12.1 ([#61](https://github.com/win-son-dev/house-of-chess/issues/61)) — Initial migration ✅
- [x] 12.1.1 (BE) `InitialCreate` migration with `Users`, `Games`, `GameMoves` + unique indices
- [x] 12.1.2 (BE) `Database.MigrateAsync()` invoked on startup

### Story 12.2 ([#62](https://github.com/win-son-dev/house-of-chess/issues/62)) — Schema for post-game data ⏳
- [ ] 12.2.1 (BE) `RatingHistory` entity + migration (see F9.2.2)
- [ ] 12.2.2 (BE) `AnalysisReport` entity + migration (see F10.2.3)
- [ ] 12.2.3 (BE) `AntiCheatFlag` entity + migration (see F10.3.2)
- [ ] 12.2.4 (BE) `Game.EndedReason` column (Checkmate / Resign / Timeout / Draw-Agreed / Draw-Auto / Aborted)

### Story 12.3 ([#63](https://github.com/win-son-dev/house-of-chess/issues/63)) — Migration strategy in prod ⏳
- [ ] 12.3.1 (Ops) Gate `Database.MigrateAsync` to `Development` env only; for prod, add a one-shot `migrate` compose service (or a manual `dotnet ef database update` step in deploy)
- [ ] 12.3.2 (Docs) Note the strategy in README

### Story 12.4 ([#64](https://github.com/win-son-dev/house-of-chess/issues/64)) — Dev seed (optional) ⏳
- [ ] 12.4.1 (BE) Seeder for a handful of test users gated by `Development` env

---

## F13 ([#13](https://github.com/win-son-dev/house-of-chess/issues/13)) — Deployment to DigitalOcean ⏳

The portfolio centerpiece — stateless API + Redis backplane + nginx LB running on a droplet.

### Story 13.1 ([#65](https://github.com/win-son-dev/house-of-chess/issues/65)) — Droplet provisioning ⏳
- [ ] 13.1.1 (Ops) Create droplet (Ubuntu 24.04, 2 GB+)
- [ ] 13.1.2 (Ops) Install Docker + compose plugin
- [ ] 13.1.3 (Ops) Domain + DNS + Let's Encrypt (Caddy in front of nginx, or certbot directly)

### Story 13.2 ([#66](https://github.com/win-son-dev/house-of-chess/issues/66)) — First deploy ⏳
- [ ] 13.2.1 (Ops) Clone repo, fill `docker/.env`, `docker compose up -d --build`
- [ ] 13.2.2 (Ops) Add prod callback URLs to Auth0 + add the prod API audience
- [ ] 13.2.3 (Ops) Smoke-test login → lobby → play one move

### Story 13.3 ([#67](https://github.com/win-son-dev/house-of-chess/issues/67)) — CI → deploy pipeline (v1.5 stretch) ⏳
- [ ] 13.3.1 (Ops) GitHub Action that SSHes on tag push and runs `git pull && docker compose up -d --build`

---

## F14 ([#14](https://github.com/win-son-dev/house-of-chess/issues/14)) — Polish & QA ⏳

Final pass before declaring v1 done.

### Story 14.1 ([#68](https://github.com/win-son-dev/house-of-chess/issues/68)) — Lichess-style UI polish ⏳
- [ ] 14.1.1 (FE) Lobby: dense seek table + live games tab alongside time-control buttons
- [ ] 14.1.2 (FE) Game view: 3-column tight (move list ‖ board ‖ side panel)
- [ ] 14.1.3 (FE) Move-played + game-end sound effects (toggleable)
- [ ] 14.1.4 (FE) Premove support in chessground (v1.5 stretch)

### Story 14.2 ([#69](https://github.com/win-son-dev/house-of-chess/issues/69)) — Accessibility ⏳
- [ ] 14.2.1 (FE) Keyboard navigation for time-control buttons + login

### Story 14.3 ([#70](https://github.com/win-son-dev/house-of-chess/issues/70)) — Docs ⏳
- [ ] 14.3.1 (Ops) README architecture diagram (multi-replica + Redis backplane topology)
- [ ] 14.3.2 (Ops) Short demo GIF (the F4.4 multi-replica showcase)

---

## F15 ([#15](https://github.com/win-son-dev/house-of-chess/issues/15)) — Observability & Operations ⏳

Cross-cutting concerns the original backlog never tracked. Several of these are pre-requisites for F13 (deploying anything to a real droplet without flying blind).

### Story 15.1 ([#71](https://github.com/win-son-dev/house-of-chess/issues/71)) — Structured logging ⏳
- [ ] 15.1.1 (BE) Replace default `ILogger` console output with **Serilog** (console sink for dev, JSON-to-stdout for prod so Docker can collect it)
- [ ] 15.1.2 (BE) Enrich logs with correlation ID (per-request middleware), userId (from JWT), gameId (where relevant)
- [ ] 15.1.3 (BE) Log every move submission + rejection at Info; every uncaught exception at Error via the existing `GlobalExceptionHandler`

### Story 15.2 ([#72](https://github.com/win-son-dev/house-of-chess/issues/72)) — Health & readiness ⏳
- [x] 15.2.1 (BE) Postgres + Redis health checks wired
- [ ] 15.2.2 (BE) Expose `/healthz` (liveness) and `/readyz` (readiness, which fails until migrations applied)
- [ ] 15.2.3 (Ops) Update `nginx.conf` to use `/healthz` for upstream health probes

### Story 15.3 ([#73](https://github.com/win-son-dev/house-of-chess/issues/73)) — Rate limiting ⏳
- [ ] 15.3.1 (BE) ASP.NET Core `RateLimiter` middleware on `/api/account/onboarding` (3/min/IP) and on `GameHub.EnqueueMatch` (10/min/user) to prevent abuse
- [ ] 15.3.2 (BE) Thresholds in config (`RateLimitOptions`)

### Story 15.4 ([#74](https://github.com/win-son-dev/house-of-chess/issues/74)) — Error response shape ⏳
- [ ] 15.4.1 (BE) Confirm `GlobalExceptionHandler` returns RFC 7807 `ProblemDetails` consistently
- [ ] 15.4.2 (BE) `ValidationProblemDetails` for 400s from model validation

### Story 15.5 ([#75](https://github.com/win-son-dev/house-of-chess/issues/75)) — Secrets handling ⏳
- [ ] 15.5.1 (Ops) `docker/.env.example` documenting every required env var; ensure `docker/.env` is gitignored
- [ ] 15.5.2 (Ops) Move Auth0 client secret, DB password, JWT signing keys (if any) into env-only (never `appsettings.json`)

---

## F16 ([#16](https://github.com/win-son-dev/house-of-chess/issues/16)) — Developer Experience ⏳

Small but high-leverage stuff so the public repo looks polished.

### Story 16.1 ([#76](https://github.com/win-son-dev/house-of-chess/issues/76)) — Lint & format ⏳
- [ ] 16.1.1 (FE) Add Prettier; CI step `npx prettier --check`
- [ ] 16.1.2 (FE) Tighten ESLint config (existing config is `eslint --init` default)
- [ ] 16.1.3 (BE) `dotnet format` step in CI

### Story 16.2 ([#77](https://github.com/win-son-dev/house-of-chess/issues/77)) — Pre-commit hooks ⏳
- [ ] 16.2.1 Husky + lint-staged on the `web/` side (prettier + eslint on staged files)
- [ ] 16.2.2 Pre-commit hook for `dotnet format` on staged `.cs`

### Story 16.3 ([#78](https://github.com/win-son-dev/house-of-chess/issues/78)) — README & contributor docs ⏳
- [ ] 16.3.1 Expand README with: prereqs, local dev steps, env-var table, "how to run a single test", troubleshooting
- [ ] 16.3.2 ARCHITECTURE.md explaining the stateless-replica + Redis-backplane choice (links from README)

---

## Suggested execution order

Dependency-respecting order to finish v1 in the fewest blocked-on-something steps. **Each line = one PR-sized chunk** that you can pick off the top.

1. **F2.3.4 + F2.3.5 + F2.3.6** — username uniqueness / 409 on onboarding (small, unblocks 2.4 + 2.5).
2. **F2.5** — `GET /api/account/me` + FE `MeProvider` consumes it.
3. **F2.4** — FE onboarding page (depends on 2.5).
4. **F2.6** — onboarding test coverage.
5. **F3.2** — PGN export service + populate `Game.Pgn` on finish.
6. **F3.3** — engine wrapper tests (cheap, high value).
7. **F12.2** — `RatingHistory` / `AnalysisReport` / `AntiCheatFlag` / `Game.EndedReason` migrations (unblocks F9.2, F6, F10).
8. **F4.1.2 + F4.1.3** — server-side clocks + atomic FEN+clock Lua swap.
9. **F4.2.4 + F4.2.5 + F4.2.6** — `clockUpdate` / `gameEnded` / `moveRejected` SignalR messages + matching FE handlers (F4.3.5–4.3.8).
10. **F6.3** — `ClockTickService` for timeouts (depends on F4.1.2).
11. **F9.2** — apply Elo on game end + `RatingHistory` row + include delta in `gameEnded` payload.
12. **F4.5** — integration test the full move flow end-to-end.
13. **F4.4** — multi-replica showcase verification + capture GIF.
14. **F6.1 + F6.2 + F6.4** — resign / draw / abort state machines.
15. **F5.1.3 + F5.1.4** — rating-bucket matchmaking (depends on real Elo updates landing).
16. **F15.1 + F15.2 + F15.3** — Serilog, `/healthz`, rate limiting (before deploy).
17. **F7** — spectator mode.
18. **F8 + F11** — history list, replay, PGN download, profile page.
19. **F10** — Stockfish analysis + anti-cheat (background, doesn't block UX).
20. **F13** — deploy to droplet.
21. **F14 + F16** — polish, lint/format, docs.

Stretch (v1.5): F9.3 (provisional K), F13.3 (CI deploy), F14.1.4 (premove), F14.3 (architecture diagram), F16.

---

## Audit notes (2026-05-26)

Changes since the previous version of this file:

- **F2 restructured.** Old F2.3 (Auth0 Action + custom claim) and the M2M / Management API tasks in old F2.4 were dropped — the decision (memory `project_house_of_chess.md`) is to look up users by `auth0_sub` in Postgres on each request. New F2.3 covers the sub-lookup onboarding endpoint that's already in code; new F2.4 is the FE onboarding flow; new F2.5 adds `GET /api/account/me`; new F2.6 collects the test work.
- **F3 reclassified ✅.** The chess engine is delegated to the `Chess` NuGet package via `MoveEngine.cs` — no in-house bitboards. Remaining work is PGN export (F3.2) and wrapper tests (F3.3).
- **F4 marked 🔄.** `SubmitMoveAsync` and `moveApplied` work today. Clocks, `clockUpdate`, `gameEnded`, `moveRejected`, optimistic UI, and end-to-end SignalR tests still pending.
- **F5 marked 🔄.** Naive pairing works; rating-bucket pairing (5.1.3) and initial Redis state on pair (5.1.4) pending.
- **F12 split.** Initial migration shipped; F12.2 collects the three missing tables (RatingHistory / AnalysisReport / AntiCheatFlag) plus a `Game.EndedReason` column; F12.3 adds prod migration strategy.
- **F15 added.** Logging, health endpoints, rate limiting, error shape, secrets handling — none were tracked before.
- **F16 added.** Lint/format/pre-commit/README polish — not tracked before, low effort, high portfolio value.
