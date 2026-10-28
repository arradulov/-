using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<ActionResult<List<DroneDto>>> GetDrones()
        {
            return await ExecuteAsync(() => service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DroneDto>> GetDrone(int id)
        {
            return await ExecuteAsync(() => service.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DroneDto>> AddDrone([FromBody] DroneCreateDto dto)
        {
            return await ExecuteCreatedAsync(() => service.CreateAsync(dto), nameof(GetDrone), d => new { id = d.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DroneDto>> UpdateDrone(int id, [FromBody] DroneUpdateDto dto)
        {
            return await ExecuteAsync(() => service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteDrone(int id)
        {
            return await ExecuteNoContentAsync(() => service.DeleteAsync(id));
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DroneDto>> Activate(int id)
        {
            return await ExecuteAsync(() => service.ActivateAsync(id));
        }

        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DroneDto>> Deactivate(int id)
        {
            return await ExecuteAsync(() => service.DeactivateAsync(id));
        }

        [HttpPost("{id}/charge")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DroneDto>> Charge(int id, [FromBody] ChargeDto dto)
        {
            return await ExecuteAsync(() => service.ChargeAsync(id, dto.Percent));
        }
    }
}
