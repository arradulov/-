using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Services
{
    // Сводные отчёты. Фильтрация и Join выполняются в БД асинхронно (ToListAsync),
    // а бизнес-методы моделей (GetAverageReward, IsSuccessful, GetAverageSpeed) — это C#-код,
    // поэтому они вызываются уже после загрузки данных, в памяти.
    public class AnalyticsService
    {
        private readonly AppDbContext db;

        public AnalyticsService(AppDbContext db)
        {
            this.db = db;
        }

        // 1. Обучения конкретного дрона: Drone + TrainingSession
        public async Task<List<DroneTrainingDto>> GetDroneTrainingsAsync(int droneId)
        {
            var drone = await db.Drones.FirstOrDefaultAsync(d => d.Id == droneId)
                ?? throw new NotFoundException($"Drone {droneId} not found");

            var sessions = await db.TrainingSessions
                .Where(s => s.DroneId == droneId)
                .OrderBy(s => s.Id)
                .ToListAsync();

            return sessions
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
        public async Task<List<DroneFlightDto>> GetDroneFlightsAsync(int droneId)
        {
            var drone = await db.Drones.FirstOrDefaultAsync(d => d.Id == droneId)
                ?? throw new NotFoundException($"Drone {droneId} not found");

            var rows = await db.TrainingSessions
                .Where(s => s.DroneId == droneId)
                .Join(db.FlightResults,
                      s => s.Id,
                      f => f.TrainingSessionId,
                      (s, f) => new { Session = s, Flight = f })
                .OrderBy(x => x.Flight.Id)
                .ToListAsync();

            return rows
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
        public async Task<List<SuccessfulFlightDto>> GetSuccessfulFlightsAsync()
        {
            var rows = await db.FlightResults
                .Where(f => f.IsFinished)
                .Join(db.TrainingSessions, f => f.TrainingSessionId, s => s.Id, (f, s) => new { Flight = f, Session = s })
                .Join(db.Drones, x => x.Session.DroneId, d => d.Id, (x, d) => new { x.Flight, x.Session, Drone = d })
                .ToListAsync();

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
        public async Task<List<AlgorithmStatsDto>> GetAlgorithmStatisticsAsync()
        {
            var sessions = await db.TrainingSessions.Where(s => s.Episodes > 0).ToListAsync();
            var finishedFlights = await db.FlightResults.Where(f => f.IsFinished).ToListAsync();

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
