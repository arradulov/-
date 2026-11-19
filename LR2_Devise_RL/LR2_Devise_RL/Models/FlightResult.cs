namespace LR2_Devise_RL.Models
{
    public class FlightResult
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int MissionId { get; set; }
        public double FlightTime { get; set; }
        public double ActualDistance { get; set; }
        public FlightStatus Status { get; set; } = FlightStatus.InProgress;

        public double CalculateAverageSpeed()
        {
            if (FlightTime <= 0)
            {
                throw new ArgumentException("Время полёта должно быть больше нуля.");
            }
            return ActualDistance / FlightTime;
        }

        public bool IsSuccessful(Mission mission)
        {
            return Status == FlightStatus.Completed
                && mission.IsAchieved(ActualDistance, FlightTime);
        }
    }
}
