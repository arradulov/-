namespace LR2_Devise_RL.Models
{
    public class Device
    {
        public const int MinimumBatteryLevel = 20;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BatteryLevel { get; set; }
        public double MaxSpeed { get; set; }
        public double MinSpeed { get; set; }

        public bool CanStartMission()
        {
            return BatteryLevel >= MinimumBatteryLevel;
        }
    }
}
