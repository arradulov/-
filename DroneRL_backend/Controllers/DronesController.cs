using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Models;

namespace DroneRL_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DronesController : ControllerBase
    {
        private readonly AppDbContext db;

        public DronesController(AppDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public List<Drone> GetDrones()
        {
            return db.Drones.OrderBy(d => d.Id).ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Drone> GetDrone(int id)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == id);
            if (drone == null)
                return NotFound(new { message = $"Drone {id} not found" });
            return drone;
        }

        [HttpPost]
        public ActionResult<Drone> AddDrone([FromBody] Drone drone)
        {
            if (string.IsNullOrWhiteSpace(drone.Name))
                return BadRequest(new { message = "Name is required" });
            if (drone.BatteryLevel < 0 || drone.BatteryLevel > 100)
                return BadRequest(new { message = "BatteryLevel must be from 0 to 100" });

            // Активность включаем только через бизнес-метод Activate()
            bool wantActive = drone.IsActive;
            drone.IsActive = false;
            if (wantActive && !drone.Activate())
                return BadRequest(new { message = $"Cannot activate drone: battery below {Drone.MinBatteryToActivate}%" });

            drone.Id = 0; // id выдаёт база данных
            db.Drones.Add(drone);
            db.SaveChanges();
            return CreatedAtAction(nameof(GetDrone), new { id = drone.Id }, drone);
        }

        [HttpPut("{id}")]
        public ActionResult<Drone> UpdateDrone(int id, [FromBody] Drone data)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == id);
            if (drone == null)
                return NotFound(new { message = $"Drone {id} not found" });
            if (string.IsNullOrWhiteSpace(data.Name))
                return BadRequest(new { message = "Name is required" });
            if (data.BatteryLevel < 0 || data.BatteryLevel > 100)
                return BadRequest(new { message = "BatteryLevel must be from 0 to 100" });

            drone.Name = data.Name;
            drone.Model = data.Model;
            drone.BatteryLevel = data.BatteryLevel;
            if (data.IsActive)
            {
                if (!drone.Activate())
                    return BadRequest(new { message = $"Cannot activate drone: battery below {Drone.MinBatteryToActivate}%" });
            }
            else
                drone.Deactivate();

            db.SaveChanges();
            return drone;
        }

        // Обучения и полёты дрона удаляются каскадно (настроено в AppDbContext)
        [HttpDelete("{id}")]
        public IActionResult DeleteDrone(int id)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == id);
            if (drone == null)
                return NotFound(new { message = $"Drone {id} not found" });

            db.Drones.Remove(drone);
            db.SaveChanges();
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public ActionResult<Drone> Activate(int id)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == id);
            if (drone == null)
                return NotFound(new { message = $"Drone {id} not found" });
            if (!drone.Activate())
                return BadRequest(new { message = $"Cannot activate drone: battery {drone.BatteryLevel}% is below {Drone.MinBatteryToActivate}%" });
            db.SaveChanges();
            return drone;
        }

        [HttpPost("{id}/deactivate")]
        public ActionResult<Drone> Deactivate(int id)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == id);
            if (drone == null)
                return NotFound(new { message = $"Drone {id} not found" });
            drone.Deactivate();
            db.SaveChanges();
            return drone;
        }

        [HttpPost("{id}/charge/{percent}")]
        public ActionResult<Drone> Charge(int id, int percent)
        {
            var drone = db.Drones.FirstOrDefault(d => d.Id == id);
            if (drone == null)
                return NotFound(new { message = $"Drone {id} not found" });
            if (percent <= 0)
                return BadRequest(new { message = "percent must be positive" });
            drone.Charge(percent);
            db.SaveChanges();
            return drone;
        }
    }
}
