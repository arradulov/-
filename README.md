# DroneRL_backend — Web API для экспериментов по обучению дрона

## Тема лабораторной
Разработка приложения Web API на .NET 8: модель из трёх классов с бизнес-логикой, CRUD-контроллеры и аналитические запросы с использованием LINQ.

## Связь с дипломом
Дипломный проект посвящён обучению с подкреплением (Reinforcement Learning) для управления БПЛА в ROS 2 и Gazebo.
Этот API хранит и анализирует эксперименты: какие дроны обучались, каким алгоритмом (PPO, DQN, SAC), с какой наградой,
и как обученная политика показала себя в тестовых полётах.

Связи: `Drone` 1 → N `TrainingSession` 1 → N `FlightResult`.

## Модели и бизнес-логика

### Drone (`Models/Drone.cs`)
Свойства: `Id`, `Name`, `Model`, `BatteryLevel`, `IsActive`.
- `Activate()` — нельзя активировать дрон с зарядом ниже 20 %.
- `Deactivate()` — выключить дрон.
- `Charge(percent)` — зарядить (не больше 100 %).
- `ConsumeBattery(percent)` — списать заряд при полёте; если заряда не хватает — отказ; при падении ниже 20 % дрон выключается.

### TrainingSession (`Models/TrainingSession.cs`)
Свойства: `Id`, `DroneId`, `Algorithm`, `Episodes`, `TotalReward`, `IsCompleted`, `StartedAt`.
- `AddEpisode(reward)` — добавить эпизод (нельзя после завершения).
- `GetAverageReward()` — средняя награда за эпизод.
- `Complete()` — завершить обучение (нельзя, если не было ни одного эпизода).

### FlightResult (`Models/FlightResult.cs`)
Свойства: `Id`, `TrainingSessionId`, `Distance` (м), `FlightTime` (с), `Collisions`, `TargetReached`.
- `RegisterCollision()` — зафиксировать столкновение.
- `ReachTarget()` — отметить достижение цели.
- `GetAverageSpeed()` — средняя скорость, м/с.
- `IsSuccessful()` — цель достигнута и нет столкновений.
- `GetBatteryCost()` — расход батареи на полёт (1 % за 30 с).

Правила при создании полёта (`POST /api/FlightResults`): обучение должно быть завершено, дрон — активен, и у него должно хватить заряда (`ConsumeBattery`).

## CRUD endpoint

| Метод | Маршрут | Описание |
|---|---|---|
| GET | `/api/Drones` | все дроны |
| GET | `/api/Drones/{id}` | дрон по id |
| POST | `/api/Drones` | добавить дрон |
| PUT | `/api/Drones/{id}` | изменить дрон |
| DELETE | `/api/Drones/{id}` | удалить дрон (вместе с его обучениями и полётами) |
| POST | `/api/Drones/{id}/activate` | `Activate()` |
| POST | `/api/Drones/{id}/deactivate` | `Deactivate()` |
| POST | `/api/Drones/{id}/charge/{percent}` | `Charge()` |
| GET | `/api/TrainingSessions` | все обучения |
| GET | `/api/TrainingSessions/{id}` | обучение по id |
| POST | `/api/TrainingSessions` | создать обучение |
| PUT | `/api/TrainingSessions/{id}` | изменить обучение |
| DELETE | `/api/TrainingSessions/{id}` | удалить обучение (вместе с полётами) |
| POST | `/api/TrainingSessions/{id}/episode/{reward}` | `AddEpisode()` |
| POST | `/api/TrainingSessions/{id}/complete` | `Complete()` |
| GET | `/api/FlightResults` | все полёты |
| GET | `/api/FlightResults/{id}` | полёт по id |
| POST | `/api/FlightResults` | добавить полёт |
| PUT | `/api/FlightResults/{id}` | изменить полёт |
| DELETE | `/api/FlightResults/{id}` | удалить полёт |
| POST | `/api/FlightResults/{id}/collision` | `RegisterCollision()` |
| POST | `/api/FlightResults/{id}/reachTarget` | `ReachTarget()` |

Несуществующий id → `404 Not Found`, нарушение бизнес-правила → `400 Bad Request`,
создание → `201 Created`, удаление → `204 No Content`.

## Аналитические endpoint (`Controllers/AnalyticsController.cs`)

| Маршрут | Что объединяет | LINQ |
|---|---|---|
| `GET /api/Analytics/drone/{droneId}/trainings` | Drone + TrainingSession | `Where`, `Select`; использует `GetAverageReward()` |
| `GET /api/Analytics/drone/{droneId}/flights` | Drone + TrainingSession + FlightResult | `Where`, `Join`, `Select`; `GetAverageReward()`, `IsSuccessful()` |
| `GET /api/Analytics/successfulFlights` | FlightResult + TrainingSession + Drone | `Where(f => f.IsSuccessful())`, `Join`, `Select`, `OrderByDescending`; `GetAverageSpeed()` |
| `GET /api/Analytics/algorithms` | TrainingSession + FlightResult | `Where`, `GroupBy`, `Select`, `Average`, `Count` |

## Где используется LINQ `Where` и `Select`
- `AnalyticsController` — во всех четырёх запросах (см. таблицу выше).
- Во всех контроллерах — `FirstOrDefault`, `Any`, `OrderBy` (переводятся EF Core в SQL-запросы к БД).

## Хранение данных
База данных **SQL Server LocalDB** (устанавливается вместе с Visual Studio) через **Entity Framework Core**.
- `AppDbContext.cs` — контекст БД: таблицы `Drones`, `TrainingSessions`, `FlightResults`, внешние ключи с каскадным удалением
  (удалили дрон → удалились его обучения и полёты) и тестовые данные (`HasData`).
- Строка подключения — `ConnectionStrings:DefaultConnection` в `appsettings.json` (база `DroneRLDb`).
- При старте вызывается `EnsureCreated()`: если базы нет, она создаётся вместе с тестовыми данными
  (3 дрона, 4 обучения, 4 полёта). Миграции не нужны.
- Контроллеры получают `AppDbContext` через конструктор и сохраняют изменения через `SaveChanges()`.
- Чтобы вернуть базу к исходным тестовым данным, поставьте `"RecreateDatabaseOnStartup": true` в `appsettings.json`
  и перезапустите проект (база будет пересоздаваться при каждом запуске, пока флаг включён).
- Посмотреть таблицы: Visual Studio → «Вид» → «Обозреватель объектов SQL Server» → `(localdb)\MSSQLLocalDB` → `DroneRLDb`.

В аналитике фильтрация и `Join` выполняются в БД (переводятся в SQL), а бизнес-методы моделей
(`GetAverageReward`, `IsSuccessful`, `GetAverageSpeed`) вызываются уже после `ToList()`, т.к. это C#-код.

## Как запустить
1. Открыть `DroneRL_backend.sln` в Visual Studio 2022 (версия 17.8 или новее, установлена рабочая нагрузка «ASP.NET и разработка веб-приложений»).
2. Выбрать профиль запуска **DroneRL_backend** и нажать **F5**.
3. Откроется Swagger: `https://localhost:5001/swagger`.

Из консоли: `dotnet run --project DroneRL_backend` и открыть `http://localhost:5000/swagger`.

## Проверка через Postman или Bruno
Файл `DroneRL_backend.postman_collection.json` — готовая коллекция всех запросов (переменная `baseUrl = http://localhost:5000`).
- **Postman:** Import → выбрать файл.
- **Bruno:** Import Collection → Postman Collection → выбрать файл.

Запросы разложены по папкам 1–8; если выполнять их по порядку на чистой базе (только тестовые данные), все коды ответа совпадут с указанными в названиях.

## Сценарий для защиты
1. `POST /api/Drones` — `{"name":"Demo Drone","model":"X500","batteryLevel":80,"isActive":true}`
2. `POST /api/TrainingSessions` — `{"droneId":4,"algorithm":"PPO"}`
3. `POST /api/TrainingSessions/5/episode/90` (пару раз), затем `POST /api/TrainingSessions/5/complete`
4. `POST /api/FlightResults` — `{"trainingSessionId":5,"distance":300,"flightTime":60,"collisions":0,"targetReached":true}`
5. Аналитика: `/api/Analytics/drone/4/flights`, `/api/Analytics/successfulFlights`, `/api/Analytics/algorithms`.
