using System;
using System.Linq;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Models
{
    public class TrainingSession
    {
        public static readonly string[] SupportedAlgorithms = { "PPO", "DQN", "SAC", "TD3", "A2C", "DDPG" };

        public int Id { get; set; }
        public int DroneId { get; set; }
        public string Algorithm { get; set; }
        public int Episodes { get; set; }
        public double TotalReward { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }

        public static string NormalizeAlgorithm(string algorithm)
        {
            var name = (algorithm ?? "").Trim().ToUpper();
            if (!SupportedAlgorithms.Contains(name))
                throw new BusinessRuleException($"Unknown algorithm '{algorithm}'. Supported: {string.Join(", ", SupportedAlgorithms)}");
            return name;
        }

        // Алгоритм можно сменить только до первого эпизода — иначе результаты обучения смешаются
        public void ChangeAlgorithm(string algorithm)
        {
            if (Episodes > 0)
                throw new BusinessRuleException("Cannot change algorithm after training has started");
            Algorithm = NormalizeAlgorithm(algorithm);
        }

        public void AddEpisode(double reward)
        {
            if (IsCompleted)
                throw new BusinessRuleException("Training is already completed");
            Episodes++;
            TotalReward += reward;
        }

        public double GetAverageReward()
        {
            if (Episodes == 0)
                return 0;
            return Math.Round(TotalReward / Episodes, 2);
        }

        public void Complete()
        {
            if (IsCompleted)
                throw new BusinessRuleException("Training is already completed");
            if (Episodes == 0)
                throw new BusinessRuleException("Cannot complete training without episodes");
            IsCompleted = true;
        }
    }
}
