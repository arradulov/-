using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<TrainingSessionDto> GetAll()
        {
            return db.TrainingSessions
                .OrderBy(s => s.Id)
                .ToList()
                .Select(TrainingSessionDto.From)
                .ToList();
        }

        public TrainingSessionDto GetById(int id)
        {
            return TrainingSessionDto.From(Find(id));
        }

        public TrainingSessionDto Create(TrainingSessionCreateDto dto)
        {
            if (!db.Drones.Any(d => d.Id == dto.DroneId))
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
            db.SaveChanges();
            return TrainingSessionDto.From(session);
        }

        public TrainingSessionDto Update(int id, TrainingSessionUpdateDto dto)
        {
            var session = Find(id);
            session.ChangeAlgorithm(dto.Algorithm);
            db.SaveChanges();
            return TrainingSessionDto.From(session);
        }

        // Полёты этого обучения удаляются каскадно
        public void Delete(int id)
        {
            var session = Find(id);
            if (db.FlightResults.Any(f => f.TrainingSessionId == id && !f.IsFinished))
                throw new BusinessRuleException("Cannot delete training: its test flight is in progress");
            db.TrainingSessions.Remove(session);
            db.SaveChanges();
        }

        public TrainingSessionDto AddEpisode(int id, double reward)
        {
            var session = Find(id);
            session.AddEpisode(reward);
            db.SaveChanges();
            return TrainingSessionDto.From(session);
        }

        public TrainingSessionDto Complete(int id)
        {
            var session = Find(id);
            session.Complete();
            db.SaveChanges();
            return TrainingSessionDto.From(session);
        }

        private TrainingSession Find(int id)
        {
            return db.TrainingSessions.FirstOrDefault(s => s.Id == id)
                ?? throw new NotFoundException($"TrainingSession {id} not found");
        }
    }
}
