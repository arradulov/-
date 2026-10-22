using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace DroneRL_backend.Controllers
{
    // Бизнес-методы моделей (GetAverageReward, IsSuccessful, GetAverageSpeed) — это C#-код,
    // база данных их не понимает. Поэтому сначала забираем данные из БД (ToList),
    // а затем обрабатываем их LINQ-запросами в памяти.
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext db;

        public AnalyticsController(AppDbContext db)
        {
            this.db = db;
        }

        // 1. Обучения конкретного дрона: Drone + TrainingSession
        [HttpGet("drone/{droneId}/trainings")]
        public ActionResult<IEnumerable<object>> GetDroneTrainings(int droneId)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == droneId);
            if (drone == null)
                return NotFound(new { message = $"Drone {droneId} not found" });

            var result = db.TrainingSessions
                .Where(s => s.DroneId == droneId)   // фильтр выполняется в БД
                .ToList()
                .Select(s => new
                {
                    droneName = drone.Name,
                    sessionId = s.Id,
                    algorithm = s.Algorithm,
                    episodes = s.Episodes,
                    averageReward = s.GetAverageReward(),
                    status = s.IsCompleted ? "Completed" : "In progress"
                })
                .ToList();
            return result;
        }

        // 2. Полёты конкретного дрона: Drone + TrainingSession + FlightResult (Join)
        [HttpGet("drone/{droneId}/flights")]
        public ActionResult<IEnumerable<object>> GetDroneFlights(int droneId)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == droneId);
            if (drone == null)
                return NotFound(new { message = $"Drone {droneId} not found" });

            var result = db.TrainingSessions
                .Where(s => s.DroneId == droneId)
                .Join(db.FlightResults,
                      s => s.Id,
                      f => f.TrainingSessionId,
                      (s, f) => new { Session = s, Flight = f })   // JOIN выполняется в БД
                .ToList()
                .Select(x => new
                {
                    drone = drone.Name,
                    flightId = x.Flight.Id,
                    algorithm = x.Session.Algorithm,
                    averageReward = x.Session.GetAverageReward(),
                    distance = x.Flight.Distance,
                    flightTime = x.Flight.FlightTime,
                    collisions = x.Flight.Collisions,
                    targetReached = x.Flight.TargetReached,
                    isSuccessful = x.Flight.IsSuccessful()
                })
                .OrderBy(x => x.flightId)
                .ToList();
            return result;
        }

        // 3. Успешные полёты всех дронов
        [HttpGet("successfulFlights")]
        public IEnumerable<object> GetSuccessfulFlights()
        {
            var flights = db.FlightResults.ToList();
            var sessions = db.TrainingSessions.ToList();
            var drones = db.Drones.ToList();

            return flights
                .Where(f => f.IsSuccessful())
                .Join(sessions,
                      f => f.TrainingSessionId,
                      s => s.Id,
                      (f, s) => new { Flight = f, Session = s })
                .Join(drones,
                      x => x.Session.DroneId,
                      d => d.Id,
                      (x, d) => new { x.Flight, x.Session, Drone = d })
                .Select(x => new
                {
                    flightId = x.Flight.Id,
                    drone = x.Drone.Name,
                    algorithm = x.Session.Algorithm,
                    distance = x.Flight.Distance,
                    flightTime = x.Flight.FlightTime,
                    averageSpeed = x.Flight.GetAverageSpeed()
                })
                .OrderByDescending(x => x.averageSpeed)
                .ToList();
        }

        // 4. Статистика по алгоритмам обучения
        [HttpGet("algorithms")]
        public IEnumerable<object> GetAlgorithmsStatistics()
        {
            var sessions = db.TrainingSessions.ToList();
            var flights = db.FlightResults.ToList();

            return sessions
                .Where(s => s.Episodes > 0)
                .GroupBy(s => s.Algorithm)
                .Select(g => new
                {
                    algorithm = g.Key,
                    experiments = g.Count(),
                    averageEpisodes = Math.Round(g.Average(s => s.Episodes), 1),
                    averageReward = Math.Round(g.Average(s => s.GetAverageReward()), 2),
                    successfulFlights = flights
                        .Where(f => g.Select(s => s.Id).Contains(f.TrainingSessionId))
                        .Count(f => f.IsSuccessful())
                })
                .OrderByDescending(x => x.averageReward)
                .ToList();
        }
    }
}
