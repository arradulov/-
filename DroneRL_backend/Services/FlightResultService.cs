using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<FlightResultDto>> GetAllAsync()
        {
            var flights = await db.FlightResults.OrderBy(f => f.Id).ToListAsync();
            return flights.Select(FlightResultDto.From).ToList();
        }

        public async Task<FlightResultDto> GetByIdAsync(int id)
        {
            return FlightResultDto.From(await FindAsync(id));
        }

        // Тестовый полёт можно начать только после завершённого обучения,
        // на активном дроне, который сейчас не находится в другом полёте
        public async Task<FlightResultDto> StartAsync(FlightStartDto dto)
        {
            var session = await db.TrainingSessions.FirstOrDefaultAsync(s => s.Id == dto.TrainingSessionId)
                ?? throw new BusinessRuleException($"TrainingSession {dto.TrainingSessionId} not found");
            if (!session.IsCompleted)
                throw new BusinessRuleException("Test flight is possible only after training is completed");

            var drone = await db.Drones.FirstAsync(d => d.Id == session.DroneId);
            if (!drone.IsActive)
                throw new BusinessRuleException($"Drone '{drone.Name}' is not active");
            if (await db.IsDroneFlyingAsync(drone.Id))
                throw new BusinessRuleException($"Drone '{drone.Name}' is already in flight");

            var flight = new FlightResult { TrainingSessionId = session.Id };
            db.FlightResults.Add(flight);
            await db.SaveChangesAsync();
            return FlightResultDto.From(flight);
        }

        public async Task<FlightResultDto> RegisterCollisionAsync(int id)
        {
            var flight = await FindAsync(id);
            flight.RegisterCollision();
            await db.SaveChangesAsync();
            return FlightResultDto.From(flight);
        }

        public async Task<FlightResultDto> ReachTargetAsync(int id)
        {
            var flight = await FindAsync(id);
            flight.ReachTarget();
            await db.SaveChangesAsync();
            return FlightResultDto.From(flight);
        }

        // Завершение полёта: записываем телеметрию и списываем заряд дрона
        public async Task<FlightResultDto> FinishAsync(int id, FlightFinishDto dto)
        {
            var flight = await FindAsync(id);
            flight.Finish(dto.Distance, dto.FlightTime);

            var drone = await db.TrainingSessions
                .Where(s => s.Id == flight.TrainingSessionId)
                .Join(db.Drones, s => s.DroneId, d => d.Id, (s, d) => d)
                .FirstAsync();
            drone.ConsumeBattery(flight.GetBatteryCost());

            await db.SaveChangesAsync(); // одной транзакцией сохраняются и полёт, и новый заряд дрона
            return FlightResultDto.From(flight);
        }

        public async Task DeleteAsync(int id)
        {
            var flight = await FindAsync(id);
            db.FlightResults.Remove(flight);
            await db.SaveChangesAsync();
        }

        private async Task<FlightResult> FindAsync(int id)
        {
            return await db.FlightResults.FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new NotFoundException($"FlightResult {id} not found");
        }
    }
}
