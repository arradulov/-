using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Dtos;
using DroneRL_backend.Models;
using DroneRL_backend.Services;

namespace DroneRL_backend.Controllers
{
    // Чтение доступно всем, изменение данных — только администратору
    [Route("api/[controller]")]
    [ApiController]
    public class DronesController : ApiControllerBase
    {
        private readonly DroneService service;

        public DronesController(DroneService service)
        {
            this.service = service;
        }

        [HttpGet]
        public ActionResult<List<DroneDto>> GetDrones()
        {
            return Execute(() => service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<DroneDto> GetDrone(int id)
        {
            return Execute(() => service.GetById(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<DroneDto> AddDrone([FromBody] DroneCreateDto dto)
        {
            return ExecuteCreated(() => service.Create(dto), nameof(GetDrone), d => new { id = d.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<DroneDto> UpdateDrone(int id, [FromBody] DroneUpdateDto dto)
        {
            return Execute(() => service.Update(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult DeleteDrone(int id)
        {
            return ExecuteNoContent(() => service.Delete(id));
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<DroneDto> Activate(int id)
        {
            return Execute(() => service.Activate(id));
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<DroneDto> Deactivate(int id)
        {
            return Execute(() => service.Deactivate(id));
        }

        [HttpPost("{id}/charge")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<DroneDto> Charge(int id, [FromBody] ChargeDto dto)
        {
            return Execute(() => service.Charge(id, dto.Percent));
        }
    }
}
