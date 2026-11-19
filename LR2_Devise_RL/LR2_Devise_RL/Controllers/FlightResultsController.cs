using LR2_Devise_RL.Data;
using LR2_Devise_RL.Models;
using LR2_Devise_RL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LR2_Devise_RL.Controllers
{
    [ApiController]
    [Route("api/flightresults")]
    public class FlightResultsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Manager _manager;

        public FlightResultsController(AppDbContext context, Manager manager)
        {
            _context = context;
            _manager = manager;
        }

        public class StartFlightRequest
        {
            public int DeviceId { get; set; }
            public int MissionId { get; set; }
        }

        public class CompleteFlightRequest
        {
            public double FlightTime { get; set; }
            public double ActualDistance { get; set; }
        }

        [HttpGet]
        public List<FlightResult> GetAll()
        {
            return _context.FlightResults.ToList();
        }
        [HttpGet("{id:int}")]
        public ActionResult<FlightResult> GetById(int id)
        {
            var flightresults = _context.FlightResults.Find(id);

            if (flightresults == null)
            {
                return NotFound();
            }

            return Ok(flightresults);
        }
        [HttpPost("start")]
        [Authorize(Roles = "admin")]
        public ActionResult<FlightResult> Start([FromBody] StartFlightRequest request)
        {
            try
            {
                var result = _manager.StartFlight(request.DeviceId, request.MissionId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }

        [HttpPut("{id:int}/complete")]
        [Authorize(Roles = "admin")]
        public ActionResult<FlightResult> Complete(
            int id,
            [FromBody] CompleteFlightRequest request)
        {
            try
            {
                return Ok(_manager.CompleteFlight(
                    id,
                    request.FlightTime,
                    request.ActualDistance));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }

        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "admin")]
        public ActionResult<FlightResult> Cancel(int id)
        {
            try
            {
                return Ok(_manager.CancelFlight(id));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var result = _context.FlightResults.Find(id);

            if (result == null)
            {
                return NotFound();
            }

            _context.FlightResults.Remove(result);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
