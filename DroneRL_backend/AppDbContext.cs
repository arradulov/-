using System;
using Microsoft.EntityFrameworkCore;
using DroneRL_backend.Models;

namespace DroneRL_backend
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Drone> Drones { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<FlightResult> FlightResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связи: Drone 1 -> N TrainingSession 1 -> N FlightResult (каскадное удаление)
            modelBuilder.Entity<TrainingSession>()
                .HasOne<Drone>()
                .WithMany()
                .HasForeignKey(s => s.DroneId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FlightResult>()
                .HasOne<TrainingSession>()
                .WithMany()
                .HasForeignKey(f => f.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Тестовые данные, которые добавляются при создании базы
            modelBuilder.Entity<Drone>().HasData(
                new Drone() { Id = 1, Name = "DJI Test Drone", Model = "DJI Mavic 3", BatteryLevel = 85, IsActive = true },
                new Drone() { Id = 2, Name = "Gazebo Simulation Drone", Model = "PX4 Iris (SITL)", BatteryLevel = 100, IsActive = true },
                new Drone() { Id = 3, Name = "Reserve Drone", Model = "Crazyflie 2.1", BatteryLevel = 10, IsActive = false }
            );

            modelBuilder.Entity<TrainingSession>().HasData(
                new TrainingSession() { Id = 1, DroneId = 1, Algorithm = "PPO", Episodes = 500, TotalReward = 42500, IsCompleted = true, StartedAt = new DateTime(2026, 9, 1, 10, 0, 0) },
                new TrainingSession() { Id = 2, DroneId = 1, Algorithm = "DQN", Episodes = 300, TotalReward = 18000, IsCompleted = true, StartedAt = new DateTime(2026, 9, 5, 14, 30, 0) },
                new TrainingSession() { Id = 3, DroneId = 2, Algorithm = "SAC", Episodes = 800, TotalReward = 72000, IsCompleted = true, StartedAt = new DateTime(2026, 9, 10, 9, 0, 0) },
                new TrainingSession() { Id = 4, DroneId = 2, Algorithm = "PPO", Episodes = 120, TotalReward = 7200, IsCompleted = false, StartedAt = new DateTime(2026, 10, 1, 12, 0, 0) }
            );

            modelBuilder.Entity<FlightResult>().HasData(
                new FlightResult() { Id = 1, TrainingSessionId = 1, Distance = 250, FlightTime = 50, Collisions = 0, TargetReached = true },   // успешный
                new FlightResult() { Id = 2, TrainingSessionId = 2, Distance = 180, FlightTime = 60, Collisions = 2, TargetReached = true },   // со столкновениями
                new FlightResult() { Id = 3, TrainingSessionId = 2, Distance = 90, FlightTime = 45, Collisions = 0, TargetReached = false },   // цель не достигнута
                new FlightResult() { Id = 4, TrainingSessionId = 3, Distance = 400, FlightTime = 64, Collisions = 0, TargetReached = true }    // успешный
            );
        }
    }
}
