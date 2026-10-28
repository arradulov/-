using System.Collections.Generic;
using System.Linq;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;
using DroneRL_backend.Models;

namespace DroneRL_backend.Services
{
    public class FlightResultService
    {
        private readonly AppDbContext db;

        public FlightResultService(AppDbContext db)
        {
            this.db = db;
        }

        public List<FlightResultDto> GetAll()
        {
            return db.FlightResults
                .OrderBy(f => f.Id)
                .ToList()
                .Select(FlightResultDto.From)
                .ToList();
        }

        public FlightResultDto GetById(int id)
        {
            return FlightResultDto.From(Find(id));
        }

        // Тестовый полёт можно начать только после завершённого обучения,
        // на активном дроне, который сейчас не находится в другом полёте
        public FlightResultDto Start(FlightStartDto dto)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == dto.TrainingSessionId)
                ?? throw new BusinessRuleException($"TrainingSession {dto.TrainingSessionId} not found");
            if (!session.IsCompleted)
                throw new BusinessRuleException("Test flight is possible only after training is completed");

            var drone = db.Drones.First(d => d.Id == session.DroneId);
            if (!drone.IsActive)
                throw new BusinessRuleException($"Drone '{drone.Name}' is not active");
            if (db.IsDroneFlying(drone.Id))
                throw new BusinessRuleException($"Drone '{drone.Name}' is already in flight");

            var flight = new FlightResult { TrainingSessionId = session.Id };
            db.FlightResults.Add(flight);
            db.SaveChanges();
            return FlightResultDto.From(flight);
        }

        public FlightResultDto RegisterCollision(int id)
        {
            var flight = Find(id);
            flight.RegisterCollision();
            db.SaveChanges();
            return FlightResultDto.From(flight);
        }

        public FlightResultDto ReachTarget(int id)
        {
            var flight = Find(id);
            flight.ReachTarget();
            db.SaveChanges();
            return FlightResultDto.From(flight);
        }

        // Завершение полёта: записываем телеметрию и списываем заряд дрона
        public FlightResultDto Finish(int id, FlightFinishDto dto)
        {
            var flight = Find(id);
            flight.Finish(dto.Distance, dto.FlightTime);

            var drone = db.TrainingSessions
                .Where(s => s.Id == flight.TrainingSessionId)
                .Join(db.Drones, s => s.DroneId, d => d.Id, (s, d) => d)
                .First();
            drone.ConsumeBattery(flight.GetBatteryCost());

            db.SaveChanges(); // одной транзакцией сохраняются и полёт, и новый заряд дрона
            return FlightResultDto.From(flight);
        }

        public void Delete(int id)
        {
            var flight = Find(id);
            db.FlightResults.Remove(flight);
            db.SaveChanges();
        }

        private FlightResult Find(int id)
        {
            return db.FlightResults.FirstOrDefault(f => f.Id == id)
                ?? throw new NotFoundException($"FlightResult {id} not found");
        }
    }
}
