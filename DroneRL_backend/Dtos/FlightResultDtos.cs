using System.ComponentModel.DataAnnotations;
using DroneRL_backend.Models;

namespace DroneRL_backend.Dtos
{
    public class FlightResultDto
    {
        public int Id { get; set; }
        public int TrainingSessionId { get; set; }
        public double Distance { get; set; }
        public double FlightTime { get; set; }
        public int Collisions { get; set; }
        public bool TargetReached { get; set; }
        public bool IsFinished { get; set; }
        public double AverageSpeed { get; set; }
        public bool IsSuccessful { get; set; }

        public static FlightResultDto From(FlightResult f) => new FlightResultDto
        {
            Id = f.Id,
            TrainingSessionId = f.TrainingSessionId,
            Distance = f.Distance,
            FlightTime = f.FlightTime,
            Collisions = f.Collisions,
            TargetReached = f.TargetReached,
            IsFinished = f.IsFinished,
            AverageSpeed = f.GetAverageSpeed(),
            IsSuccessful = f.IsSuccessful()
        };
    }

    // Старт тестового полёта: указывается только обучение, остальное заполняется по ходу полёта
    public class FlightStartDto
    {
        [Range(1, int.MaxValue)]
        public int TrainingSessionId { get; set; }
    }

    // Завершение полёта: телеметрия
    public class FlightFinishDto
    {
        [Range(0, 100000)]
        public double Distance { get; set; }

        [Range(0.1, 86400)]
        public double FlightTime { get; set; }
    }
}
