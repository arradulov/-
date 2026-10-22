using System;

namespace DroneRL_backend.Models
{
    public class Drone
    {
        public const int MinBatteryToActivate = 20;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int BatteryLevel { get; set; } = 100;
        public bool IsActive { get; set; }

        // Нельзя активировать дрон с низким зарядом
        public bool Activate()
        {
            if (BatteryLevel < MinBatteryToActivate)
                return false;
            IsActive = true;
            return true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Charge(int percent)
        {
            BatteryLevel = Math.Min(100, BatteryLevel + Math.Max(0, percent));
        }

        // Списывает заряд; если заряда не хватает — ничего не меняет и возвращает false
        public bool ConsumeBattery(int percent)
        {
            if (percent < 0 || percent > BatteryLevel)
                return false;
            BatteryLevel -= percent;
            if (BatteryLevel < MinBatteryToActivate)
                Deactivate();
            return true;
        }
    }
}
