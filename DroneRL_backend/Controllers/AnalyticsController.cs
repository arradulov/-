using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DroneRL_backend.Dtos;
using DroneRL_backend.Services;

namespace DroneRL_backend.Controllers
{
    // Сводные отчёты доступны только авторизованным пользователям (любая роль)
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyticsController : ApiControllerBase
    {
        private readonly AnalyticsService service;

        public AnalyticsController(AnalyticsService service)
        {
            this.service = service;
        }

        [HttpGet("drone/{droneId}/trainings")]
        public ActionResult<List<DroneTrainingDto>> GetDroneTrainings(int droneId)
        {
            return Execute(() => service.GetDroneTrainings(droneId));
        }

        [HttpGet("drone/{droneId}/flights")]
        public ActionResult<List<DroneFlightDto>> GetDroneFlights(int droneId)
        {
            return Execute(() => service.GetDroneFlights(droneId));
        }

        [HttpGet("successfulFlights")]
        public ActionResult<List<SuccessfulFlightDto>> GetSuccessfulFlights()
        {
            return Execute(() => service.GetSuccessfulFlights());
        }

        [HttpGet("algorithms")]
        public ActionResult<List<AlgorithmStatsDto>> GetAlgorithmsStatistics()
        {
            return Execute(() => service.GetAlgorithmStatistics());
        }
    }
}
