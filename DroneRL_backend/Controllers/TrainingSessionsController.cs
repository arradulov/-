using System.Collections.Generic;
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
        public ActionResult<List<TrainingSessionDto>> GetSessions()
        {
            return Execute(() => service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<TrainingSessionDto> GetSession(int id)
        {
            return Execute(() => service.GetById(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<TrainingSessionDto> AddSession([FromBody] TrainingSessionCreateDto dto)
        {
            return ExecuteCreated(() => service.Create(dto), nameof(GetSession), s => new { id = s.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<TrainingSessionDto> UpdateSession(int id, [FromBody] TrainingSessionUpdateDto dto)
        {
            return Execute(() => service.Update(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult DeleteSession(int id)
        {
            return ExecuteNoContent(() => service.Delete(id));
        }

        [HttpPost("{id}/episode")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<TrainingSessionDto> AddEpisode(int id, [FromBody] EpisodeDto dto)
        {
            return Execute(() => service.AddEpisode(id, dto.Reward));
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = Roles.Admin)]
        public ActionResult<TrainingSessionDto> Complete(int id)
        {
            return Execute(() => service.Complete(id));
        }
    }
}
