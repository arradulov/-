using LR2_Devise_RL.Data;
using LR2_Devise_RL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LR2_Devise_RL.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReportsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("flights")]
        public IActionResult GetFlights()
        {
            var report = _context.FlightResults
                .Join(
                    _context.Devices,
                    result => result.DeviceId,
                    device => device.Id,
                    (result, device) => new { result, device }
                )
                .Join(
                    _context.Missions,
                    flight => flight.result.MissionId,
                    mission => mission.Id,
                    (flight, mission) => new
                    {
                        Id = flight.result.Id,
                        DeviceName = flight.device.Name,
                        MissionName = mission.Name,
                        FlightTime = flight.result.FlightTime,
                        ActualDistance = flight.result.ActualDistance,
                        Status = flight.result.Status,
                        AverageSpeed = flight.result.FlightTime > 0
                            ? flight.result.ActualDistance / flight.result.FlightTime
                            : 0,
                        IsSuccessful = flight.result.Status == FlightStatus.Completed
                            && flight.result.ActualDistance >= mission.Distance
                            && flight.result.FlightTime > 0
                            && flight.result.FlightTime <= mission.TimeLimit
                    }
                )
                .ToList();

            return Ok(report);
        }
        [HttpGet("successful-flights")]
        public IActionResult GetSuccessfulFlights()
        {
            var report = _context.FlightResults
                .Join(
                    _context.Devices,
                    result => result.DeviceId,
                    device => device.Id,
                    (result, device) => new { result, device }
                )
                .Join(
                    _context.Missions,
                    flight => flight.result.MissionId,
                    mission => mission.Id,
                    (flight, mission) => new { flight.result, flight.device, mission }
                )
                .Where(item => item.result.Status == FlightStatus.Completed
                    && item.result.ActualDistance >= item.mission.Distance
                    && item.result.FlightTime > 0
                    && item.result.FlightTime <= item.mission.TimeLimit)
                .Select(item => new
                {
                    Id = item.result.Id,
                    DeviceName = item.device.Name,
                    MissionName = item.mission.Name,
                    FlightTime = item.result.FlightTime,
                    ActualDistance = item.result.ActualDistance,
                    AverageSpeed = item.result.ActualDistance / item.result.FlightTime
                })
                .ToList();

            return Ok(report);
        }
    }
}
