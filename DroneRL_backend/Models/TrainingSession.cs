using System;

namespace DroneRL_backend.Models
{
    public class TrainingSession
    {
        public int Id { get; set; }
        public int DroneId { get; set; }
        public string Algorithm { get; set; }
        public int Episodes { get; set; }
        public double TotalReward { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.Now;

        // Добавить результат одного эпизода обучения (нельзя после завершения)
        public bool AddEpisode(double reward)
        {
            if (IsCompleted)
                return false;
            Episodes++;
            TotalReward += reward;
            return true;
        }

        public double GetAverageReward()
        {
            if (Episodes == 0)
                return 0;
            return Math.Round(TotalReward / Episodes, 2);
        }

        // Нельзя завершить обучение без единого эпизода
        public bool Complete()
        {
            if (Episodes == 0)
                return false;
            IsCompleted = true;
            return true;
        }
    }
}
