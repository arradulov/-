using Microsoft.AspNetCore.Mvc;
using LR2_Devise_RL.Data;
using LR2_Devise_RL.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;


namespace LR2_Devise_RL.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public List<Device> GetAll()
        {
            return _context.Devices.ToList();
        }

        [HttpGet("{id:int}")]
        public ActionResult<Device> GetById(int id)
        {
            var device = _context.Devices.Find(id);

            if (device == null)
            {
                return NotFound();
            }

            return Ok(device);
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult<Device> Create([FromBody] Device device)
        {
            if (string.IsNullOrWhiteSpace(device.Name))
            {
                return BadRequest("Название устройства обязательно.");
            }

            if (device.BatteryLevel < 0 || device.BatteryLevel > 100)
            {
                return BadRequest("Заряд должен быть от 0 до 100%.");
            }

            if (device.MinSpeed < 0 || device.MaxSpeed < 0)
            {
                return BadRequest("Скорость не может быть отрицательной.");
            }

            if (device.MinSpeed > device.MaxSpeed)
            {
                return BadRequest("Минимальная скорость не может превышать максимальную.");
            }

            device.Id = 0;

            _context.Devices.Add(device);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Device device)
        {
            if (string.IsNullOrWhiteSpace(device.Name))
            {
                return BadRequest("Название устройства обязательно.");
            }

            if (device.BatteryLevel < 0 || device.BatteryLevel > 100)
            {
                return BadRequest("Заряд должен быть от 0 до 100%.");
            }

            if (device.MinSpeed < 0 || device.MaxSpeed < 0)
            {
                return BadRequest("Скорость не может быть отрицательной.");
            }

            if (device.MinSpeed > device.MaxSpeed)
            {
                return BadRequest("Минимальная скорость не может превышать максимальную.");
            }
            var existingDevice = _context.Devices.Find(id);

            if (existingDevice == null)
            {
                return NotFound();
            }
            existingDevice.Name = device.Name;
            existingDevice.BatteryLevel = device.BatteryLevel;
            existingDevice.MinSpeed = device.MinSpeed;
            existingDevice.MaxSpeed = device.MaxSpeed;

            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var existingDevice = _context.Devices.Find(id);
            if (existingDevice == null) { 
                return NotFound();
            }

            if (_context.FlightResults.Any(result => result.DeviceId == id))
            {
                return Conflict("Устройство связано с полётами. Удаление запрещено.");
            }

            _context.Devices.Remove(existingDevice);
            _context.SaveChanges();

            return NoContent();
        }

    }
}
