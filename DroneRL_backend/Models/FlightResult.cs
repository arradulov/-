using System;

namespace DroneRL_backend.Models
{
    public class FlightResult
    {
        public int Id { get; set; }
        public int TrainingSessionId { get; set; }
        public double Distance { get; set; }    // метры
        public double FlightTime { get; set; }  // секунды
        public int Collisions { get; set; }
        public bool TargetReached { get; set; }

        public void RegisterCollision()
        {
            Collisions++;
        }

        public void ReachTarget()
        {
            TargetReached = true;
        }

        // Средняя скорость, м/с
        public double GetAverageSpeed()
        {
            if (FlightTime <= 0)
                return 0;
            return Math.Round(Distance / FlightTime, 2);
        }

        // Успешный полёт: цель достигнута и не было столкновений
        public bool IsSuccessful()
        {
            return TargetReached && Collisions == 0;
        }

        // Расход батареи на полёт: 1% за каждые 30 секунд
        public int GetBatteryCost()
        {
            return (int)Math.Ceiling(FlightTime / 30);
        }
    }
}
