using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Models;

namespace DroneRL_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingSessionsController : ControllerBase
    {
        private readonly AppDbContext db;

        public TrainingSessionsController(AppDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public List<TrainingSession> GetSessions()
        {
            return db.TrainingSessions.OrderBy(s => s.Id).ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<TrainingSession> GetSession(int id)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return NotFound(new { message = $"TrainingSession {id} not found" });
            return session;
        }

        [HttpPost]
        public ActionResult<TrainingSession> AddSession([FromBody] TrainingSession session)
        {
            if (!db.Drones.Any(d => d.Id == session.DroneId))
                return BadRequest(new { message = $"Drone {session.DroneId} not found" });
            if (string.IsNullOrWhiteSpace(session.Algorithm))
                return BadRequest(new { message = "Algorithm is required" });
            if (session.Episodes < 0)
                return BadRequest(new { message = "Episodes must be >= 0" });

            // Завершать обучение можно только через Complete()
            bool wantCompleted = session.IsCompleted;
            session.IsCompleted = false;
            if (wantCompleted && !session.Complete())
                return BadRequest(new { message = "Cannot complete training without episodes" });

            session.Algorithm = session.Algorithm.ToUpper();
            if (session.StartedAt == default)
                session.StartedAt = DateTime.Now;
            session.Id = 0; // id выдаёт база данных
            db.TrainingSessions.Add(session);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        [HttpPut("{id}")]
        public ActionResult<TrainingSession> UpdateSession(int id, [FromBody] TrainingSession data)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return NotFound(new { message = $"TrainingSession {id} not found" });
            if (!db.Drones.Any(d => d.Id == data.DroneId))
                return BadRequest(new { message = $"Drone {data.DroneId} not found" });
            if (string.IsNullOrWhiteSpace(data.Algorithm))
                return BadRequest(new { message = "Algorithm is required" });
            if (data.Episodes < 0)
                return BadRequest(new { message = "Episodes must be >= 0" });
            if (data.IsCompleted && data.Episodes == 0)
                return BadRequest(new { message = "Cannot complete training without episodes" });

            session.DroneId = data.DroneId;
            session.Algorithm = data.Algorithm.ToUpper();
            session.Episodes = data.Episodes;
            session.TotalReward = data.TotalReward;
            session.IsCompleted = false;
            if (data.IsCompleted)
                session.Complete();
            if (data.StartedAt != default)
                session.StartedAt = data.StartedAt;

            db.SaveChanges();
            return session;
        }

        // Полёты этого обучения удаляются каскадно
        [HttpDelete("{id}")]
        public IActionResult DeleteSession(int id)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return NotFound(new { message = $"TrainingSession {id} not found" });

            db.TrainingSessions.Remove(session);
            db.SaveChanges();
            return NoContent();
        }

        [HttpPost("{id}/episode/{reward}")]
        public ActionResult<TrainingSession> AddEpisode(int id, double reward)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return NotFound(new { message = $"TrainingSession {id} not found" });
            if (!session.AddEpisode(reward))
                return BadRequest(new { message = "Training is already completed" });
            db.SaveChanges();
            return session;
        }

        [HttpPost("{id}/complete")]
        public ActionResult<TrainingSession> Complete(int id)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == id);
            if (session == null)
                return NotFound(new { message = $"TrainingSession {id} not found" });
            if (!session.Complete())
                return BadRequest(new { message = "Cannot complete training without episodes" });
            db.SaveChanges();
            return session;
        }
    }
}
