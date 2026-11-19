using LR2_Devise_RL.Data;
using LR2_Devise_RL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LR2_Devise_RL.Controllers
{
    [ApiController]
    [Route("api/missions")]
    public class MissionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public MissionsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id:int}")]
        public ActionResult<Mission> GetById(int id)
        {
            var mission = _context.Missions.Find(id);

            if (mission == null)
            {
                return NotFound();
            }

            return Ok(mission);
        }
        [HttpGet]
        public List<Mission> GetAll()
        {
            return _context.Missions.ToList();
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult<Mission> Create([FromBody] Mission mission)
        {
            if (string.IsNullOrWhiteSpace(mission.Name))
            {
                return BadRequest("Название миссии обязательно.");
            }
            if (mission.Distance <= 0)
            {
                return BadRequest("Расстояние должно быть больше нуля");
            }
            if (mission.TimeLimit <= 0)
            {
                return BadRequest("Лимит времени должен быть больше нуля.");
            }
            mission.Id = 0;

            _context.Missions.Add(mission);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = mission.Id }, mission);
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Mission mission)
        {
            var existingMission = _context.Missions.Find(id);

            if (existingMission == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(mission.Name))
            {
                return BadRequest("Название миссии обязательно.");
            }

            if (mission.Distance <= 0)
            {
                return BadRequest("Расстояние должно быть больше нуля.");
            }

            if (mission.TimeLimit <= 0)
            {
                return BadRequest("Лимит времени должен быть больше нуля.");
            }
            if (_context.FlightResults.Any(result => result.MissionId == id))
            {
                return Conflict("Миссия уже использовалась в полётах. Изменение и удаление запрещены.");
            }

            existingMission.Name = mission.Name;
            existingMission.Distance = mission.Distance;
            existingMission.TimeLimit = mission.TimeLimit;

            _context.SaveChanges();

            return NoContent();
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var existingMission = _context.Missions.Find(id);

            if (existingMission == null)
            {
                return NotFound();
            }
            if (_context.FlightResults.Any(result => result.MissionId == id))
            {
                return Conflict("Миссия уже использовалась в полётах. Изменение и удаление запрещены.");
            }
            _context.Missions.Remove(existingMission);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
