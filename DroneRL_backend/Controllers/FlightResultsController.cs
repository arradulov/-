using System.Collections.Generic;
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
        public ActionResult<List<FlightResultDto>> GetFlights()
        {
            return Execute(() => service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<FlightResultDto> GetFlight(int id)
        {
            return Execute(() => service.GetById(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<FlightResultDto> StartFlight([FromBody] FlightStartDto dto)
        {
            return ExecuteCreated(() => service.Start(dto), nameof(GetFlight), f => new { id = f.Id });
        }

        [HttpPost("{id}/collision")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<FlightResultDto> RegisterCollision(int id)
        {
            return Execute(() => service.RegisterCollision(id));
        }

        [HttpPost("{id}/reachTarget")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<FlightResultDto> ReachTarget(int id)
        {
            return Execute(() => service.ReachTarget(id));
        }

        [HttpPost("{id}/finish")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<FlightResultDto> FinishFlight(int id, [FromBody] FlightFinishDto dto)
        {
            return Execute(() => service.Finish(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult DeleteFlight(int id)
        {
            return ExecuteNoContent(() => service.Delete(id));
        }
    }
}
