using System;
using System.ComponentModel.DataAnnotations;
using DroneRL_backend.Models;

namespace DroneRL_backend.Dtos
{
    public class TrainingSessionDto
    {
        public int Id { get; set; }
        public int DroneId { get; set; }
        public string Algorithm { get; set; }
        public int Episodes { get; set; }
        public double TotalReward { get; set; }
        public double AverageReward { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }

        public static TrainingSessionDto From(TrainingSession s) => new TrainingSessionDto
        {
            Id = s.Id,
            DroneId = s.DroneId,
            Algorithm = s.Algorithm,
            Episodes = s.Episodes,
            TotalReward = s.TotalReward,
            AverageReward = s.GetAverageReward(),
            IsCompleted = s.IsCompleted,
            StartedAt = s.StartedAt
        };
    }

    // Новое обучение всегда начинается с нуля эпизодов;
    // эпизоды добавляются через /episode, завершение — через /complete
    public class TrainingSessionCreateDto
    {
        [Range(1, int.MaxValue)]
        public int DroneId { get; set; }

        [Required]
        public string Algorithm { get; set; }
    }

    // Изменить можно только алгоритм и только до первого эпизода
    public class TrainingSessionUpdateDto
    {
        [Required]
        public string Algorithm { get; set; }
    }

    public class EpisodeDto
    {
        public double Reward { get; set; }
    }
}
