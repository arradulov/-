using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Models;

namespace DroneRL_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightResultsController : ControllerBase
    {
        private readonly AppDbContext db;

        public FlightResultsController(AppDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public List<FlightResult> GetFlights()
        {
            return db.FlightResults.OrderBy(f => f.Id).ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<FlightResult> GetFlight(int id)
        {
            var flight = db.FlightResults.FirstOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound(new { message = $"FlightResult {id} not found" });
            return flight;
        }

        // Тестовый полёт возможен только после завершённого обучения и на активном дроне;
        // полёт расходует заряд батареи дрона
        [HttpPost]
        public ActionResult<FlightResult> AddFlight([FromBody] FlightResult flight)
        {
            var session = db.TrainingSessions.FirstOrDefault(s => s.Id == flight.TrainingSessionId);
            if (session == null)
                return BadRequest(new { message = $"TrainingSession {flight.TrainingSessionId} not found" });
            if (!session.IsCompleted)
                return BadRequest(new { message = "Test flight is possible only after training is completed" });
            if (flight.Distance < 0 || flight.FlightTime <= 0 || flight.Collisions < 0)
                return BadRequest(new { message = "Distance >= 0, FlightTime > 0, Collisions >= 0 required" });

            var drone = db.Drones.FirstOrDefault(d => d.Id == session.DroneId);
            if (drone == null)
                return BadRequest(new { message = $"Drone {session.DroneId} not found" });
            if (!drone.IsActive)
                return BadRequest(new { message = $"Drone {drone.Name} is not active" });
            if (!drone.ConsumeBattery(flight.GetBatteryCost()))
                return BadRequest(new { message = $"Not enough battery: need {flight.GetBatteryCost()}%, have {drone.BatteryLevel}%" });

            flight.Id = 0; // id выдаёт база данных
            db.FlightResults.Add(flight);
            db.SaveChanges(); // сохраняет и полёт, и новый заряд дрона
            return CreatedAtAction(nameof(GetFlight), new { id = flight.Id }, flight);
        }

        [HttpPut("{id}")]
        public ActionResult<FlightResult> UpdateFlight(int id, [FromBody] FlightResult data)
        {
            var flight = db.FlightResults.FirstOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound(new { message = $"FlightResult {id} not found" });
            if (!db.TrainingSessions.Any(s => s.Id == data.TrainingSessionId))
                return BadRequest(new { message = $"TrainingSession {data.TrainingSessionId} not found" });
            if (data.Distance < 0 || data.FlightTime <= 0 || data.Collisions < 0)
                return BadRequest(new { message = "Distance >= 0, FlightTime > 0, Collisions >= 0 required" });

            flight.TrainingSessionId = data.TrainingSessionId;
            flight.Distance = data.Distance;
            flight.FlightTime = data.FlightTime;
            flight.Collisions = data.Collisions;
            flight.TargetReached = data.TargetReached;

            db.SaveChanges();
            return flight;
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFlight(int id)
        {
            var flight = db.FlightResults.FirstOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound(new { message = $"FlightResult {id} not found" });
            db.FlightResults.Remove(flight);
            db.SaveChanges();
            return NoContent();
        }

        [HttpPost("{id}/collision")]
        public ActionResult<FlightResult> RegisterCollision(int id)
        {
            var flight = db.FlightResults.FirstOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound(new { message = $"FlightResult {id} not found" });
            flight.RegisterCollision();
            db.SaveChanges();
            return flight;
        }

        [HttpPost("{id}/reachTarget")]
        public ActionResult<FlightResult> ReachTarget(int id)
        {
            var flight = db.FlightResults.FirstOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound(new { message = $"FlightResult {id} not found" });
            flight.ReachTarget();
            db.SaveChanges();
            return flight;
        }
    }
}
