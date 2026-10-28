using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<ActionResult<List<DroneTrainingDto>>> GetDroneTrainings(int droneId)
        {
            return await ExecuteAsync(() => service.GetDroneTrainingsAsync(droneId));
        }

        [HttpGet("drone/{droneId}/flights")]
        public async Task<ActionResult<List<DroneFlightDto>>> GetDroneFlights(int droneId)
        {
            return await ExecuteAsync(() => service.GetDroneFlightsAsync(droneId));
        }

        [HttpGet("successfulFlights")]
        public async Task<ActionResult<List<SuccessfulFlightDto>>> GetSuccessfulFlights()
        {
            return await ExecuteAsync(() => service.GetSuccessfulFlightsAsync());
        }

        [HttpGet("algorithms")]
        public async Task<ActionResult<List<AlgorithmStatsDto>>> GetAlgorithmsStatistics()
        {
            return await ExecuteAsync(() => service.GetAlgorithmStatisticsAsync());
        }
    }
}
