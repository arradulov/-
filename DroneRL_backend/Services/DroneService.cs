using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;
using DroneRL_backend.Models;

namespace DroneRL_backend.Services
{
    // Вся работа с таблицей дронов. Контроллер вызывает только методы сервиса.
    // Запросы к БД асинхронные: пока база отвечает, поток сервера обслуживает другие запросы.
    public class DroneService
    {
        private readonly AppDbContext db;

        public DroneService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<List<DroneDto>> GetAllAsync()
        {
            var drones = await db.Drones.OrderBy(d => d.Id).ToListAsync();
            return drones.Select(DroneDto.From).ToList();
        }

        public async Task<DroneDto> GetByIdAsync(int id)
        {
            return DroneDto.From(await FindAsync(id));
        }

        public async Task<DroneDto> CreateAsync(DroneCreateDto dto)
        {
            var drone = new Drone
            {
                Name = dto.Name.Trim(),
                Model = dto.Model.Trim(),
                BatteryLevel = dto.BatteryLevel,
                IsActive = false
            };
            db.Drones.Add(drone);
            await db.SaveChangesAsync();
            return DroneDto.From(drone);
        }

        public async Task<DroneDto> UpdateAsync(int id, DroneUpdateDto dto)
        {
            var drone = await FindAsync(id);
            drone.Name = dto.Name.Trim();
            drone.Model = dto.Model.Trim();
            await db.SaveChangesAsync();
            return DroneDto.From(drone);
        }

        // Обучения и полёты дрона удаляются каскадно (настроено в AppDbContext)
        public async Task DeleteAsync(int id)
        {
            var drone = await FindAsync(id);
            await EnsureNotFlyingAsync(drone);
            db.Drones.Remove(drone);
            await db.SaveChangesAsync();
        }

        public async Task<DroneDto> ActivateAsync(int id)
        {
            var drone = await FindAsync(id);
            drone.Activate();
            await db.SaveChangesAsync();
            return DroneDto.From(drone);
        }

        public async Task<DroneDto> DeactivateAsync(int id)
        {
            var drone = await FindAsync(id);
            await EnsureNotFlyingAsync(drone);
            drone.Deactivate();
            await db.SaveChangesAsync();
            return DroneDto.From(drone);
        }

        public async Task<DroneDto> ChargeAsync(int id, int percent)
        {
            var drone = await FindAsync(id);
            await EnsureNotFlyingAsync(drone);
            drone.Charge(percent);
            await db.SaveChangesAsync();
            return DroneDto.From(drone);
        }

        private async Task<Drone> FindAsync(int id)
        {
            return await db.Drones.FirstOrDefaultAsync(d => d.Id == id)
                ?? throw new NotFoundException($"Drone {id} not found");
        }

        private async Task EnsureNotFlyingAsync(Drone drone)
        {
            if (await db.IsDroneFlyingAsync(drone.Id))
                throw new BusinessRuleException($"Drone '{drone.Name}' is in flight now");
        }
    }
}
