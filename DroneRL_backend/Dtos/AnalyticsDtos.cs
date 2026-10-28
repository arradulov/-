namespace DroneRL_backend.Dtos
{
    // 1. Обучения дрона (Drone + TrainingSession)
    public class DroneTrainingDto
    {
        public string DroneName { get; set; }
        public int SessionId { get; set; }
        public string Algorithm { get; set; }
        public int Episodes { get; set; }
        public double AverageReward { get; set; }
        public string Status { get; set; }
    }

    // 2. Полёты дрона (Drone + TrainingSession + FlightResult)
    public class DroneFlightDto
    {
        public string Drone { get; set; }
        public int FlightId { get; set; }
        public string Algorithm { get; set; }
        public double AverageReward { get; set; }
        public double Distance { get; set; }
        public double FlightTime { get; set; }
        public int Collisions { get; set; }
        public bool TargetReached { get; set; }
        public bool IsFinished { get; set; }
        public bool IsSuccessful { get; set; }
    }

    // 3. Успешные полёты
    public class SuccessfulFlightDto
    {
        public int FlightId { get; set; }
        public string Drone { get; set; }
        public string Algorithm { get; set; }
        public double Distance { get; set; }
        public double FlightTime { get; set; }
        public double AverageSpeed { get; set; }
    }

    // 4. Статистика по алгоритмам
    public class AlgorithmStatsDto
    {
        public string Algorithm { get; set; }
        public int Experiments { get; set; }
        public double AverageEpisodes { get; set; }
        public double AverageReward { get; set; }
        public int FinishedFlights { get; set; }
        public int SuccessfulFlights { get; set; }
        public double SuccessRate { get; set; }   // доля успешных полётов, %
    }
}
