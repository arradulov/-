# DroneRL_backend — Web API для экспериментов по обучению дрона

Лабораторная работа по Web API (.NET 8, ASP.NET Core). Связана с дипломом: обучение с подкреплением (RL)
для управления БПЛА в ROS 2 / Gazebo. API хранит дроны, сеансы обучения (PPO, DQN, SAC…) и тестовые полёты,
и строит по ним сводные отчёты.

`Drone` 1 → N `TrainingSession` 1 → N `FlightResult`

## Структура

| Папка / файл | Назначение |
|---|---|
| `Models/` | Модели с бизнес-логикой: `Drone`, `TrainingSession`, `FlightResult`, `User` |
| `Dtos/` | DTO — то, что API принимает и возвращает (модели наружу не отдаются) |
| `Services/` | **Вся работа с БД**: `DroneService`, `TrainingSessionService`, `FlightResultService`, `AnalyticsService`, `AuthService` |
| `Controllers/` | Только HTTP: принимают DTO, вызывают сервис, возвращают код ответа |
| `Controllers/ApiControllerBase.cs` | Превращает исключения бизнес-логики в 400 / 404 |
| `Exceptions/` | `BusinessRuleException` (→ 400), `NotFoundException` (→ 404) |
| `AppDbContext.cs` | EF Core: таблицы, связи (каскадное удаление), тестовые данные |
| `Startup.cs` | DI, JWT-авторизация, Swagger с кнопкой Authorize |
| `DroneRL_backend.postman_collection.json` | Коллекция запросов Postman (импортируется и в Bruno) |

## Бизнес-логика (в моделях)

**Drone** — `Activate()` (нельзя при заряде < 20 % и если уже активен), `Deactivate()`, `Charge(%)` (не выше 100 %),
`ConsumeBattery(%)` (списание после полёта; при заряде < 20 % дрон выключается).
Нельзя выключить, зарядить или удалить дрон, пока он в полёте.

**TrainingSession** — создаётся с нулём эпизодов; `AddEpisode(reward)` (нельзя после завершения),
`GetAverageReward()`, `Complete()` (нельзя без эпизодов и повторно), `ChangeAlgorithm()` (только до первого эпизода).
Алгоритм — из списка PPO, DQN, SAC, TD3, A2C, DDPG.

**FlightResult** — полёт как процесс: **старт** (только после завершённого обучения, на активном дроне,
который не летит) → `RegisterCollision()` / `ReachTarget()` → **`Finish(distance, time)`** (телеметрия + списание заряда:
1 % за 30 с). После завершения полёт не меняется. `IsSuccessful()` = завершён, цель достигнута, 0 столкновений.
`GetAverageSpeed()` = дистанция / время.

## Что убрано как нелогичное (по сравнению с первой версией)

- Нельзя через PUT задать дрону заряд или активность — только `/charge`, `/activate`, `/deactivate`, полёты.
- Нельзя при создании обучения передать готовые эпизоды/награду/«завершено» — только `/episode` и `/complete`.
- Нельзя сменить алгоритм, дрон обучения после старта; повторно завершить обучение.
- У полёта нет PUT: нельзя «переписать» телеметрию задним числом без списания заряда.
- Нельзя начать второй полёт, пока дрон в воздухе; событие столкновения после завершения полёта.

## Авторизация (JWT)

Пользователи хранятся в БД (пароль — хеш PBKDF2 с солью). Тестовые: **admin / admin123**, **user / user123**.

1. `POST /api/Auth/login` `{ "login": "admin", "password": "admin123" }` → `token`.
2. Передавать заголовок `Authorization: Bearer <token>` (в Swagger — кнопка **Authorize**).

| Что | Кто |
|---|---|
| GET дронов, обучений, полётов | все |
| Отчёты `/api/Analytics/*`, `/api/Auth/me` | любой авторизованный (иначе **401**) |
| Добавление / изменение / удаление, бизнес-операции, регистрация пользователей | только **admin** (user получит **403**) |

## Endpoint

- `api/Drones` — GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}`, POST `{id}/activate`, `{id}/deactivate`, `{id}/charge` `{percent}`
- `api/TrainingSessions` — GET, GET `{id}`, POST `{droneId, algorithm}`, PUT `{id}` `{algorithm}`, DELETE, POST `{id}/episode` `{reward}`, `{id}/complete`
- `api/FlightResults` — GET, GET `{id}`, POST `{trainingSessionId}` (старт), POST `{id}/collision`, `{id}/reachTarget`, `{id}/finish` `{distance, flightTime}`, DELETE
- `api/Auth` — POST `login`, GET `me`, POST `register` (admin), GET `users` (admin)

## Отчёты (LINQ, `Services/AnalyticsService.cs`)

| Запрос | Объединяет | LINQ |
|---|---|---|
| `GET /api/Analytics/drone/{id}/trainings` | Drone + TrainingSession | `Where`, `Select` |
| `GET /api/Analytics/drone/{id}/flights` | Drone + TrainingSession + FlightResult | `Where`, `Join`, `Select` |
| `GET /api/Analytics/successfulFlights` | FlightResult + TrainingSession + Drone | `Where`, `Join` ×2, `Select`, `OrderByDescending` |
| `GET /api/Analytics/algorithms` | TrainingSession + FlightResult | `Where`, `GroupBy`, `Select`, `Count`, `Average` |

Фильтры и `Join` выполняются в SQL; бизнес-методы (`GetAverageReward`, `IsSuccessful`, `GetAverageSpeed`) — после `ToList()`.

## База данных

SQL Server LocalDB, база `DroneRLDb_v2`, создаётся при первом запуске с тестовыми данными.
Сбросить к исходным данным: `"RecreateDatabaseOnStartup": true` в `appsettings.json` → запустить → вернуть `false`.

## Запуск

- **Visual Studio:** открыть `DroneRL_backend.sln`, **Ctrl+F5** (запуск без отладки). Swagger: http://localhost:5000/swagger
- **Release-сборка:** `dotnet publish DroneRL_backend -c Release -r win-x64 --self-contained false -o Release`,
  затем `Release\Запуск.cmd` (или `DroneRL_backend.exe`). Нужен установленный ASP.NET Core Runtime 8.

## Тестирование (Postman / Bruno)

Импортировать `DroneRL_backend.postman_collection.json` и выполнить папки **0–9 по порядку** (или Runner целиком).
Папка 0 сохраняет токены admin и user в переменные, дальше id созданных объектов тоже сохраняются в переменные —
коллекцию можно прогонять повторно. Ожидаемый код ответа указан в скобках в названии каждого запроса, и каждый запрос
проверяет его тестом. Демонстрация 401 — папка 1 (запрос без токена), 403 — запрос с токеном user к операциям admin.
