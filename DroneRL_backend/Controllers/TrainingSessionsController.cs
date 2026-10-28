using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Dtos;
using DroneRL_backend.Models;
using DroneRL_backend.Services;

namespace DroneRL_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingSessionsController : ApiControllerBase
    {
        private readonly TrainingSessionService service;

        public TrainingSessionsController(TrainingSessionService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<TrainingSessionDto>>> GetSessions()
        {
            return await ExecuteAsync(() => service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingSessionDto>> GetSession(int id)
        {
            return await ExecuteAsync(() => service.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<TrainingSessionDto>> AddSession([FromBody] TrainingSessionCreateDto dto)
        {
            return await ExecuteCreatedAsync(() => service.CreateAsync(dto), nameof(GetSession), s => new { id = s.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<TrainingSessionDto>> UpdateSession(int id, [FromBody] TrainingSessionUpdateDto dto)
        {
            return await ExecuteAsync(() => service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteSession(int id)
        {
            return await ExecuteNoContentAsync(() => service.DeleteAsync(id));
        }

        [HttpPost("{id}/episode")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<TrainingSessionDto>> AddEpisode(int id, [FromBody] EpisodeDto dto)
        {
            return await ExecuteAsync(() => service.AddEpisodeAsync(id, dto.Reward));
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<TrainingSessionDto>> Complete(int id)
        {
            return await ExecuteAsync(() => service.CompleteAsync(id));
        }
    }
}
