using LR2_Devise_RL.Data;
using LR2_Devise_RL.Models;
namespace LR2_Devise_RL.Services
{
    public class Manager
    {
        private readonly AppDbContext _context;

        public Manager(AppDbContext context)
        {
            _context = context;
        }
        public FlightResult StartFlight(int deviceId, int missionId)
        {
            var device = _context.Devices.Find(deviceId);

            if (device == null)
            {
                throw new KeyNotFoundException("Устройство не найдено.");
            }

            var mission = _context.Missions.Find(missionId);

            if (mission == null)
            {
                throw new KeyNotFoundException("Миссия не найдена.");
            }

            if (!device.CanStartMission())
            {
                throw new ArgumentException(
                    "Для запуска требуется заряд не менее 20%.");
            }

            if (mission.Distance <= 0 || mission.TimeLimit <= 0)
            {
                throw new ArgumentException(
                    "Расстояние и лимит времени миссии должны быть больше нуля.");
            }

            bool isBusy = _context.FlightResults.Any(flight =>
                flight.DeviceId == deviceId &&
                flight.Status == FlightStatus.InProgress);

            if (isBusy)
            {
                throw new InvalidOperationException(
                    "Устройство уже выполняет полёт.");
            }

            var flight = new FlightResult
            {
                DeviceId = deviceId,
                MissionId = missionId,
                Status = FlightStatus.InProgress,
                FlightTime = 0,
                ActualDistance = 0
            };

            _context.FlightResults.Add(flight);
            _context.SaveChanges();

            return flight;
        }

        public FlightResult CompleteFlight(
            int flightId,
            double flightTime,
            double actualDistance)
        {
            if (flightTime <= 0)
            {
                throw new ArgumentException("Время полёта должно быть больше нуля.");
            }

            if (actualDistance < 0)
            {
                throw new ArgumentException("Расстояние не может быть отрицательным.");
            }

            var flight = GetInProgressFlight(flightId);
            flight.FlightTime = flightTime;
            flight.ActualDistance = actualDistance;
            flight.Status = FlightStatus.Completed;

            _context.SaveChanges();
            return flight;
        }

        public FlightResult CancelFlight(int flightId)
        {
            var flight = GetInProgressFlight(flightId);
            flight.Status = FlightStatus.Cancelled;

            _context.SaveChanges();
            return flight;
        }

        private FlightResult GetInProgressFlight(int flightId)
        {
            var flight = _context.FlightResults.Find(flightId);

            if (flight == null)
            {
                throw new KeyNotFoundException("Полёт не найден.");
            }

            if (flight.Status != FlightStatus.InProgress)
            {
                throw new InvalidOperationException(
                    "Завершить или отменить можно только выполняющийся полёт.");
            }

            return flight;
        }
    }
}
