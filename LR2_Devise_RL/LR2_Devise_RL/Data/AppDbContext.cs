using Microsoft.EntityFrameworkCore;
using LR2_Devise_RL.Models;

namespace LR2_Devise_RL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<FlightResult> FlightResults { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FlightResult>()
                .HasOne<Device>()
                .WithMany()
                .HasForeignKey(result => result.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FlightResult>()
                .HasOne<Mission>()
                .WithMany()
                .HasForeignKey(result => result.MissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
