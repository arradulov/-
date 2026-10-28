using System.Collections.Generic;
using System.Linq;
using DroneRL_backend.Dtos;
using DroneRL_backend.Exceptions;
using DroneRL_backend.Models;

namespace DroneRL_backend.Services
{
    // Вся работа с таблицей дронов. Контроллер вызывает только методы сервиса.
    public class DroneService
    {
        private readonly AppDbContext db;

        public DroneService(AppDbContext db)
        {
            this.db = db;
        }

        public List<DroneDto> GetAll()
        {
            return db.Drones
                .OrderBy(d => d.Id)
                .ToList()
                .Select(DroneDto.From)
                .ToList();
        }

        public DroneDto GetById(int id)
        {
            return DroneDto.From(Find(id));
        }

        public DroneDto Create(DroneCreateDto dto)
        {
            var drone = new Drone
            {
                Name = dto.Name.Trim(),
                Model = dto.Model.Trim(),
                BatteryLevel = dto.BatteryLevel,
                IsActive = false
            };
            db.Drones.Add(drone);
            db.SaveChanges();
            return DroneDto.From(drone);
        }

        public DroneDto Update(int id, DroneUpdateDto dto)
        {
            var drone = Find(id);
            drone.Name = dto.Name.Trim();
            drone.Model = dto.Model.Trim();
            db.SaveChanges();
            return DroneDto.From(drone);
        }

        // Обучения и полёты дрона удаляются каскадно (настроено в AppDbContext)
        public void Delete(int id)
        {
            var drone = Find(id);
            EnsureNotFlying(drone);
            db.Drones.Remove(drone);
            db.SaveChanges();
        }

        public DroneDto Activate(int id)
        {
            var drone = Find(id);
            drone.Activate();
            db.SaveChanges();
            return DroneDto.From(drone);
        }

        public DroneDto Deactivate(int id)
        {
            var drone = Find(id);
            EnsureNotFlying(drone);
            drone.Deactivate();
            db.SaveChanges();
            return DroneDto.From(drone);
        }

        public DroneDto Charge(int id, int percent)
        {
            var drone = Find(id);
            EnsureNotFlying(drone);
            drone.Charge(percent);
            db.SaveChanges();
            return DroneDto.From(drone);
        }

        private Drone Find(int id)
        {
            return db.Drones.FirstOrDefault(d => d.Id == id)
                ?? throw new NotFoundException($"Drone {id} not found");
        }

        private void EnsureNotFlying(Drone drone)
        {
            if (db.IsDroneFlying(drone.Id))
                throw new BusinessRuleException($"Drone '{drone.Name}' is in flight now");
        }
    }
}
