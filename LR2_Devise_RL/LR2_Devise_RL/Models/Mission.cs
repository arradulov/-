namespace LR2_Devise_RL.Models
{
    public class Mission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double TimeLimit { get; set; }

        public bool IsAchieved(double actualDistance, double flightTime)
        {
            if (actualDistance < 0 || flightTime <= 0)
            {
                throw new ArgumentException(
                    "Расстояние не может быть отрицательным, а время должно быть больше нуля.");
            }

            return actualDistance >= Distance && flightTime <= TimeLimit;
        }
    }
}
