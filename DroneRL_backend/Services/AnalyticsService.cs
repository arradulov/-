using System;
using System.Collections.Generic;
using System.Linq;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Services
{
    // Сводные отчёты. Фильтрация и Join выполняются в БД (EF Core переводит их в SQL),
    // а бизнес-методы моделей (GetAverageReward, IsSuccessful, GetAverageSpeed) — это C#-код,
    // поэтому они вызываются после ToList(), уже в памяти.
    public class AnalyticsService
    {
        private readonly AppDbContext db;

        public AnalyticsService(AppDbContext db)
        {
            this.db = db;
        }

        // 1. Обучения конкретного дрона: Drone + TrainingSession
        public List<DroneTrainingDto> GetDroneTrainings(int droneId)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == droneId)
                ?? throw new NotFoundException($"Drone {droneId} not found");

            return db.TrainingSessions
                .Where(s => s.DroneId == droneId)
                .OrderBy(s => s.Id)
                .ToList()
                .Select(s => new DroneTrainingDto
                {
                    DroneName = drone.Name,
                    SessionId = s.Id,
                    Algorithm = s.Algorithm,
                    Episodes = s.Episodes,
                    AverageReward = s.GetAverageReward(),
                    Status = s.IsCompleted ? "Completed" : "In progress"
                })
                .ToList();
        }

        // 2. Полёты конкретного дрона: Drone + TrainingSession + FlightResult
        public List<DroneFlightDto> GetDroneFlights(int droneId)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == droneId)
                ?? throw new NotFoundException($"Drone {droneId} not found");

            return db.TrainingSessions
                .Where(s => s.DroneId == droneId)
                .Join(db.FlightResults,
                      s => s.Id,
                      f => f.TrainingSessionId,
                      (s, f) => new { Session = s, Flight = f })
                .OrderBy(x => x.Flight.Id)
                .ToList()
                .Select(x => new DroneFlightDto
                {
                    Drone = drone.Name,
                    FlightId = x.Flight.Id,
                    Algorithm = x.Session.Algorithm,
                    AverageReward = x.Session.GetAverageReward(),
                    Distance = x.Flight.Distance,
                    FlightTime = x.Flight.FlightTime,
                    Collisions = x.Flight.Collisions,
                    TargetReached = x.Flight.TargetReached,
                    IsFinished = x.Flight.IsFinished,
                    IsSuccessful = x.Flight.IsSuccessful()
                })
                .ToList();
        }

        // 3. Успешные полёты всех дронов, отсортированные по средней скорости
        public List<SuccessfulFlightDto> GetSuccessfulFlights()
        {
            var rows = db.FlightResults
                .Where(f => f.IsFinished)
                .Join(db.TrainingSessions, f => f.TrainingSessionId, s => s.Id, (f, s) => new { Flight = f, Session = s })
                .Join(db.Drones, x => x.Session.DroneId, d => d.Id, (x, d) => new { x.Flight, x.Session, Drone = d })
                .ToList();

            return rows
                .Where(x => x.Flight.IsSuccessful())
                .Select(x => new SuccessfulFlightDto
                {
                    FlightId = x.Flight.Id,
                    Drone = x.Drone.Name,
                    Algorithm = x.Session.Algorithm,
                    Distance = x.Flight.Distance,
                    FlightTime = x.Flight.FlightTime,
                    AverageSpeed = x.Flight.GetAverageSpeed()
                })
                .OrderByDescending(x => x.AverageSpeed)
                .ToList();
        }

        // 4. Статистика по алгоритмам обучения
        public List<AlgorithmStatsDto> GetAlgorithmStatistics()
        {
            var sessions = db.TrainingSessions.Where(s => s.Episodes > 0).ToList();
            var finishedFlights = db.FlightResults.Where(f => f.IsFinished).ToList();

            return sessions
                .GroupBy(s => s.Algorithm)
                .Select(g =>
                {
                    var sessionIds = g.Select(s => s.Id).ToList();
                    var flights = finishedFlights.Where(f => sessionIds.Contains(f.TrainingSessionId)).ToList();
                    int successful = flights.Count(f => f.IsSuccessful());
                    return new AlgorithmStatsDto
                    {
                        Algorithm = g.Key,
                        Experiments = g.Count(),
                        AverageEpisodes = Math.Round(g.Average(s => s.Episodes), 1),
                        AverageReward = Math.Round(g.Average(s => s.GetAverageReward()), 2),
                        FinishedFlights = flights.Count,
                        SuccessfulFlights = successful,
                        SuccessRate = flights.Count == 0 ? 0 : Math.Round(100.0 * successful / flights.Count, 1)
                    };
                })
                .OrderByDescending(x => x.AverageReward)
                .ToList();
        }
    }
}
