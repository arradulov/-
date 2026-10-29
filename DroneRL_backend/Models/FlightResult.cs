using System;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Models
{
    // Тестовый полёт обученного дрона.
    public class FlightResult
    {
        public int Id { get; set; }
        public int TrainingSessionId { get; set; }
        public double Distance { get; set; }    // метры
        public double FlightTime { get; set; }  // секунды
        public int Collisions { get; set; }
        public bool TargetReached { get; set; }
        public bool IsFinished { get; set; }

        public void RegisterCollision()
        {
            if (IsFinished)
                throw new BusinessRuleException("Flight is already finished");
            Collisions++;
        }

        public void ReachTarget()
        {
            if (IsFinished)
                throw new BusinessRuleException("Flight is already finished");
            if (TargetReached)
                throw new BusinessRuleException("Target is already reached");
            TargetReached = true;
        }

        public void Finish(double distance, double flightTime)
        {
            if (IsFinished)
                throw new BusinessRuleException("Flight is already finished");
            if (distance < 0 || flightTime <= 0)
                throw new BusinessRuleException("Distance must be >= 0 and FlightTime must be > 0");
            Distance = distance;
            FlightTime = flightTime;
            IsFinished = true;
        }

        // Средняя скорость, м/с
        public double GetAverageSpeed()
        {
            if (FlightTime <= 0)
                return 0;
            return Math.Round(Distance / FlightTime, 2);
        }

        // Успешный полёт
        public bool IsSuccessful()
        {
            return IsFinished && TargetReached && Collisions == 0;
        }

        // Расход батареи: 1% за каждые 30 секунд полёта
        public int GetBatteryCost()
        {
            return (int)Math.Ceiling(FlightTime / 30);
        }
    }
}
