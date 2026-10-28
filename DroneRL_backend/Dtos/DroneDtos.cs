using System.ComponentModel.DataAnnotations;
using DroneRL_backend.Models;

namespace DroneRL_backend.Dtos
{
    // То, что API возвращает клиенту
    public class DroneDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsActive { get; set; }

        public static DroneDto From(Drone d) => new DroneDto
        {
            Id = d.Id,
            Name = d.Name,
            Model = d.Model,
            BatteryLevel = d.BatteryLevel,
            IsActive = d.IsActive
        };
    }

    // Создание дрона: новый дрон всегда выключен, включается через /activate
    public class DroneCreateDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [Required, StringLength(100, MinimumLength = 1)]
        public string Model { get; set; }

        [Range(0, 100)]
        public int BatteryLevel { get; set; } = 100;
    }

    // Изменение дрона: только описательные поля.
    // Заряд меняется через /charge и полёты, активность — через /activate и /deactivate.
    public class DroneUpdateDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [Required, StringLength(100, MinimumLength = 1)]
        public string Model { get; set; }
    }

    public class ChargeDto
    {
        [Range(1, 100)]
        public int Percent { get; set; }
    }
}
