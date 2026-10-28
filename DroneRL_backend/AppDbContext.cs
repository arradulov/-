using System;
using System.Linq;
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
        public DbSet<User> Users { get; set; }

        // Есть ли у дрона незавершённый полёт
        public bool IsDroneFlying(int droneId)
        {
            return FlightResults
                .Where(f => !f.IsFinished)
                .Join(TrainingSessions, f => f.TrainingSessionId, s => s.Id, (f, s) => s.DroneId)
                .Any(id => id == droneId);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Drone>().Property(d => d.Name).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Drone>().Property(d => d.Model).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<TrainingSession>().Property(s => s.Algorithm).HasMaxLength(10).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Login).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(10).IsRequired();
            modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();

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
                new FlightResult() { Id = 1, TrainingSessionId = 1, Distance = 250, FlightTime = 50, Collisions = 0, TargetReached = true, IsFinished = true },   // успешный
                new FlightResult() { Id = 2, TrainingSessionId = 2, Distance = 180, FlightTime = 60, Collisions = 2, TargetReached = true, IsFinished = true },   // со столкновениями
                new FlightResult() { Id = 3, TrainingSessionId = 2, Distance = 90, FlightTime = 45, Collisions = 0, TargetReached = false, IsFinished = true },   // цель не достигнута
                new FlightResult() { Id = 4, TrainingSessionId = 3, Distance = 400, FlightTime = 64, Collisions = 0, TargetReached = true, IsFinished = true }    // успешный
            );

            // Пользователи: admin / admin123 и user / user123 (хранятся только хеши паролей)
            modelBuilder.Entity<User>().HasData(
                new User() { Id = 1, Login = "admin", Role = Roles.Admin, PasswordHash = "IVbJph+Qf6bC1Ktj2W7UIA==:BkGU0/r+ES+X/NNzGuvikK8BqrbxV4FutjTp+fuPhlw=" },
                new User() { Id = 2, Login = "user", Role = Roles.User, PasswordHash = "qUGEZz3dtF4V17r+vWf//g==:BZyd1Zch4fT8BLhsWA+7tgEFdYQDMjwnF/imhY6GHcY=" }
            );
        }
    }
}
