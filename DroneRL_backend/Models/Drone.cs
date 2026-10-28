using System;
using DroneRL_backend.Exceptions;

namespace DroneRL_backend.Models
{
    public class Drone
    {
        public const int MinBatteryToActivate = 20;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsActive { get; set; }

        // Нельзя активировать уже активный дрон или дрон с низким зарядом
        public void Activate()
        {
            if (IsActive)
                throw new BusinessRuleException($"Drone '{Name}' is already active");
            if (BatteryLevel < MinBatteryToActivate)
                throw new BusinessRuleException($"Cannot activate drone: battery {BatteryLevel}% is below {MinBatteryToActivate}%");
            IsActive = true;
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new BusinessRuleException($"Drone '{Name}' is already inactive");
            IsActive = false;
        }

        public void Charge(int percent)
        {
            if (percent <= 0)
                throw new BusinessRuleException("Charge percent must be positive");
            if (BatteryLevel == 100)
                throw new BusinessRuleException("Battery is already full");
            BatteryLevel = Math.Min(100, BatteryLevel + percent);
        }

        // Полёт уже состоялся, поэтому заряд списывается в любом случае (но не ниже 0).
        // Если заряд упал ниже минимума — дрон автоматически выключается.
        public void ConsumeBattery(int percent)
        {
            BatteryLevel = Math.Max(0, BatteryLevel - percent);
            if (BatteryLevel < MinBatteryToActivate)
                IsActive = false;
        }
    }
}
