using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Dtos;
using DroneRL_backend.Models;
using DroneRL_backend.Services;

namespace DroneRL_backend.Controllers
{
    // Полёт — это процесс: POST (старт) → collision / reachTarget → finish.
    // Произвольного PUT нет: результат полёта меняется только через эти действия,
    // иначе можно было бы «переписать» телеметрию без списания заряда.
    [Route("api/[controller]")]
    [ApiController]
    public class FlightResultsController : ApiControllerBase
    {
        private readonly FlightResultService service;

        public FlightResultsController(FlightResultService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<FlightResultDto>>> GetFlights()
        {
            return await ExecuteAsync(() => service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FlightResultDto>> GetFlight(int id)
        {
            return await ExecuteAsync(() => service.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<FlightResultDto>> StartFlight([FromBody] FlightStartDto dto)
        {
            return await ExecuteCreatedAsync(() => service.StartAsync(dto), nameof(GetFlight), f => new { id = f.Id });
        }

        [HttpPost("{id}/collision")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<FlightResultDto>> RegisterCollision(int id)
        {
            return await ExecuteAsync(() => service.RegisterCollisionAsync(id));
        }

        [HttpPost("{id}/reachTarget")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<FlightResultDto>> ReachTarget(int id)
        {
            return await ExecuteAsync(() => service.ReachTargetAsync(id));
        }

        [HttpPost("{id}/finish")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<FlightResultDto>> FinishFlight(int id, [FromBody] FlightFinishDto dto)
        {
            return await ExecuteAsync(() => service.FinishAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            return await ExecuteNoContentAsync(() => service.DeleteAsync(id));
        }
    }
}
