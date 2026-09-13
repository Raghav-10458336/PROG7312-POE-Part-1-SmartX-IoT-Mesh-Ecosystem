namespace SmartX.Shared.Models
{
    public class PowerAnalysisResult
    {
        public double MeterA { get; set; }

        public double MeterB { get; set; }

        public double CombinedLoad { get; set; }

        public double LoadDifference { get; set; }

        public string HighestLoadMeter { get; set; } =
            string.Empty;
    }
}