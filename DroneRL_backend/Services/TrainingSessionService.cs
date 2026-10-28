using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;
using DroneRL_backend.Models;

namespace DroneRL_backend.Services
{
    public class TrainingSessionService
    {
        private readonly AppDbContext db;

        public TrainingSessionService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<List<TrainingSessionDto>> GetAllAsync()
        {
            var sessions = await db.TrainingSessions.OrderBy(s => s.Id).ToListAsync();
            return sessions.Select(TrainingSessionDto.From).ToList();
        }

        public async Task<TrainingSessionDto> GetByIdAsync(int id)
        {
            return TrainingSessionDto.From(await FindAsync(id));
        }

        public async Task<TrainingSessionDto> CreateAsync(TrainingSessionCreateDto dto)
        {
            if (!await db.Drones.AnyAsync(d => d.Id == dto.DroneId))
                throw new BusinessRuleException($"Drone {dto.DroneId} not found");

            var session = new TrainingSession
            {
                DroneId = dto.DroneId,
                Algorithm = TrainingSession.NormalizeAlgorithm(dto.Algorithm),
                Episodes = 0,
                TotalReward = 0,
                IsCompleted = false,
                StartedAt = DateTime.Now
            };
            db.TrainingSessions.Add(session);
            await db.SaveChangesAsync();
            return TrainingSessionDto.From(session);
        }

        public async Task<TrainingSessionDto> UpdateAsync(int id, TrainingSessionUpdateDto dto)
        {
            var session = await FindAsync(id);
            session.ChangeAlgorithm(dto.Algorithm);
            await db.SaveChangesAsync();
            return TrainingSessionDto.From(session);
        }

        // Полёты этого обучения удаляются каскадно
        public async Task DeleteAsync(int id)
        {
            var session = await FindAsync(id);
            if (await db.FlightResults.AnyAsync(f => f.TrainingSessionId == id && !f.IsFinished))
                throw new BusinessRuleException("Cannot delete training: its test flight is in progress");
            db.TrainingSessions.Remove(session);
            await db.SaveChangesAsync();
        }

        public async Task<TrainingSessionDto> AddEpisodeAsync(int id, double reward)
        {
            var session = await FindAsync(id);
            session.AddEpisode(reward);
            await db.SaveChangesAsync();
            return TrainingSessionDto.From(session);
        }

        public async Task<TrainingSessionDto> CompleteAsync(int id)
        {
            var session = await FindAsync(id);
            session.Complete();
            await db.SaveChangesAsync();
            return TrainingSessionDto.From(session);
        }

        private async Task<TrainingSession> FindAsync(int id)
        {
            return await db.TrainingSessions.FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException($"TrainingSession {id} not found");
        }
    }
}
